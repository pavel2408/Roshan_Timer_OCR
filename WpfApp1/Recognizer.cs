using System;
using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
//using Tesseract;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;

namespace WpfApp1
{
    class Recognizer
    {
        protected static int threshold_w = 240;
        protected static string path_to_tesseract = "";
        Tesseract _ocr;
        Image<Rgb, byte> i_img;
        List<String> key_words_side;

        Image<Gray, byte> outputImage;

        string key_person;
        string text;

        private string ConvertPixelsAndRecognize(Image<Rgb, byte> img)
        {
            outputImage = new Image<Gray, byte>(img.Width, img.Height);
            outputImage = img.Convert<Gray, byte>();
            //outputImage.Save("./to_recognition.jpg");
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
            key_words_side = new List<string>();
            key_words_side.Add("силы света");
            key_words_side.Add("силы тьмы");
            key_person = "рошан";
            _ocr = new Tesseract(path_to_tesseract, "rus", OcrEngineMode.Default);
        }

        private bool ParseText(string text)
        {
            string lower_txt = text.ToLower();
            
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
            i_img = b_img.ToImage<Rgb,byte>();
            text = ConvertPixelsAndRecognize(i_img);
            return ParseText(text);
        }
    }
}
