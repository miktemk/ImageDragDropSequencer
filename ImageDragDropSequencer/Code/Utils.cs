using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Net;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using ImageDragDropSequencer.ViewModel;

namespace ImageDragDropSequencer.Code
{
    public class Utils
    {
        public static Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        public static void SaveHtmlContentToImage(string htmlString, string filename)
        {
            // https://regex101.com/r/3mpopj/3
            //var regex64 = new Regex(@"<img[^>]*?src=""(data:image\/jpeg;base64.*?)""[^>]*?>");
            var regex64 = new Regex(@"<img[^>]*?src=""data:image\/jpeg;base64,(.*?)""[^>]*?>"); // LOOK: we are interested in the bit after the comma
            var match = regex64.Match(htmlString);
            if (match.Success)
            {
                var base64String = match.Groups[1].Value;
                var image64ed = Base64ToImage(base64String);
                image64ed.Save(filename, ImageFormat.Jpeg);
                image64ed.Dispose();
                return;
            }

            // https://regex101.com/r/3mpopj/1
            var regexLink = new Regex(@"<img[^>]*?src=""(.*?)""[^>]*?>");
            match = regexLink.Match(htmlString);
            if (match.Success)
            {
                var link = match.Groups[1].Value;
                // TODO: download from this link
                using (var client = new WebClient())
                {
                    const string tmpDownloadFilename = "tmp-image-download.jpg";
                    client.DownloadFile(link, tmpDownloadFilename);
                    var img = Image.FromFile(tmpDownloadFilename);
                    img.Save(filename, ImageFormat.Jpeg);
                    img.Dispose();
                    File.Delete(tmpDownloadFilename);
                    return;
                }
            }
        }

        public static DraggyImageSequenceVM LoadImageSequenceFile(string filename)
        {
            if (!File.Exists(filename))
                return null;
            var allText = File.ReadAllText(filename);
            var obj = JsonConvert.DeserializeObject<DraggyImageSequenceVM>(allText);

            return obj;
        }

        public static void SaveImageSequenceFile(DraggyImageSequenceVM seq, string filename)
        {
            var strJson = JsonConvert.SerializeObject(seq);
            File.WriteAllText(filename, strJson);
        }
    }
}
