using System;
using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
//using Tesseract;
using System.Diagnostics;
using System.Drawing;

namespace WpfApp1
{
    class Recognizer
    {
        protected static int threshold_w = 240;
        protected static string path_to_tesseract = "";

        private string ConvertPixelsAndRecognize(Image<Rgb, byte> img)
        {
            Image<Gray, byte> outputImage = new Image<Gray, byte>(img.Width, img.Height);
            outputImage = img.Convert<Gray, byte>();
            outputImage.Save("./to_recognition.jpg");
            for (int i = outputImage.Rows - 1; i >= 0; i--)
            {
                for (int j = outputImage.Cols - 1; j >= 0; j--)
                {
                    if (outputImage.Data[i, j, 0] < threshold_w)
                    {
                        outputImage.Data[i, j, 0] = 0;
                    }
                    else
                    {
                        outputImage.Data[i, j, 0] = 255;
                    }
                }
            }
            var _ocr = new Tesseract(path_to_tesseract, "rus", OcrEngineMode.Default);
            //_ocr.SetVariable("tessedit_char_whitelist", "0123456789АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя");
            _ocr.SetImage(outputImage);
            _ocr.Recognize();
            var outputText = _ocr.GetUTF8Text();
            Console.WriteLine(outputText);
            //outputImage = outputImage.Not();
            return outputText;
        }
        public Recognizer(string path_to_tes)
        {
            path_to_tesseract = path_to_tes;
        }

        private bool ParseText(string text)
        {
            string lower_txt = text.ToLower();
            string[] key_words_side = { "силы света", "силы тьмы" };
            string key_person = "рошан";
            Console.WriteLine(lower_txt);
            foreach (string i in key_words_side)
            {
                if (lower_txt.Contains(i) && lower_txt.Contains(key_person))
                    return true;
            }
            return false;
        }
        public bool StartRecognition(Bitmap b_img)
        {
            string text = "";
            Image<Rgb, byte> i_img = b_img.ToImage<Rgb,byte>();
            text = ConvertPixelsAndRecognize(i_img);
            //var path_to_img = "./to_recognize.jpg";
            //reversed_image.Save(path_to_img);
            /*try
            {
                using (var engine = new TesseractEngine(@path_to_tesseract, "rus", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(path_to_img))
                    {
                        using (var page = engine.Process(img))
                        {
                            text = page.GetText();
                            //Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                            //Console.WriteLine("Text (GetText): \r\n{0}", text);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
            }
            */
            return ParseText(text);
        }
    }
}
