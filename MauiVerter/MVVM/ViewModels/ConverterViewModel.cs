using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Translate;
using Amazon.Translate.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace MauiVerter.MVVM.ViewModels
{
    public class ConverterViewModel
    {
        public string QuantityName { get; set; }
        public ObservableCollection<string> FromMeasures { get; set; }
        public ObservableCollection<string> ToMeasures { get; set; }
        public string CurrentFromMeasure { get; set; }
        public string CurrentToMeasure { get; set; }
        public ConverterViewModel() {
            QuantityName = "Length";
            FromMeasures = LoadMeasures();
            ToMeasures = LoadMeasures();
            CurrentFromMeasure = TranslateToUk("Meter");
            CurrentToMeasure = TranslateToUk("Centimeter");
        }
        private ObservableCollection<string> LoadMeasures()
        {
            var types = 
                Quantity.Infos
                .FirstOrDefault(x => x.Name == QuantityName)
                .UnitInfos
                .Select(u => u.Name)
                .ToList();
            types.ForEach(x => x = TranslateToUk(x));
            return new ObservableCollection<string>(types);
        }
        public static string TranslateToUk(string srcText)
        {
            return Translate(srcText).Result;
        }
       
        public static string TranslateToEn(string srcText)
        {
            return Translate(srcText, false).Result;
        }
        private static async  Task<string> Translate(string srcText, bool enTouk = true)
        {
            var client = new AmazonTranslateClient();
            string srcLang = "en"; // English.
            string destLang = "uk"; // Ukraine.           
            srcLang = enTouk ? srcLang : destLang;
            destLang = enTouk ? destLang : srcLang;

            var destText = await TranslatingTextAsync(client, srcLang, destLang, srcText);
            return destText;
        }
        private static async Task<string> GetSourceTextAsync(string srcBucket, string srcTextFile)
        {
            string srcText = string.Empty;

            var s3Client = new AmazonS3Client();
            TransferUtility utility = new TransferUtility(s3Client);

            using var stream = await utility.OpenStreamAsync(srcBucket, srcTextFile);

            StreamReader file = new System.IO.StreamReader(stream);

            srcText = file.ReadToEnd();
            return srcText;
        }
        private static async Task<string> TranslatingTextAsync(AmazonTranslateClient client, string srcLang, string destLang, string text)
        {
            var request = new TranslateTextRequest
            {
                SourceLanguageCode = srcLang,
                TargetLanguageCode = destLang,
                Text = text,
            };

            var response = await client.TranslateTextAsync(request);

            return response.TranslatedText;
        }
    }
}
