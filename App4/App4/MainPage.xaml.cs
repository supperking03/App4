using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.TextToSpeech;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace App4
{
    public partial class MainPage : ContentPage
    {
        public Dictionary<string, string> dic = new Dictionary<string, string>();
        private List<string> names = new List<string>();

        public string getName(string a)
        {
            string first = new StringReader(a).ReadLine(); // read the first line
            int pos = 0;
            for (int i = 0; i < first.Length; i++) // search for the first / and find pos
            {
                if (first[i] == '/')
                {
                    pos = i - 1;
                    break;
                }
            }
            return first.Substring(0, pos); // cut name from the first line
        }

        public MainPage()
        {
            InitializeComponent();

            var assembly = typeof ( MainPage ).GetTypeInfo ( ).Assembly;
            Stream stream = assembly.GetManifestResourceStream ( "App4.tudien.txt");

            string text = "";
            using ( var reader = new System.IO.StreamReader ( stream ) )
            {
                text = reader.ReadToEnd ( );
            }

            string[] g = new string[10];
            g = text.Split('@');
            
            try
            {
                for (int i = 0; i < g.Length; i++)
                {
                    // add từ xử lý từ bị trùng
                    if (!dic.ContainsKey(getName(g[i])))
                        dic.Add(getName(g[i]), g[i]); // add new entry
                    else
                        dic[getName(g[i])] = g[i]; // update entry value

                }
            }
            catch
            {

            }

            names = new List<string>(dic.Keys);
            MainListView.ItemsSource = names;
        }

        private void Bamnut(object sender, EventArgs e)
        {
            if (Search.Text != "")
            {
                if (dic.ContainsKey(Search.Text.ToString()))
                {
                    dic[Search.Text.ToString()] = dic[Search.Text.ToString()].Replace("=", "- ");
                    Result.Text = dic[Search.Text.ToString()].Replace("+", " = ");
                }
                else
                {
                    Result.Text = "Word not found";
                }
            }
        }

        private void Search_OnSearchButtonPressed(object sender, EventArgs e)
        {
            string keyword = Search.Text;

            IEnumerable<string> searchResult = from name
                in names
                where name.Contains(keyword)
                select name;
            MainListView.ItemsSource = searchResult;
            Bamnut(sender, e);
        }

        private void Searchtext(object sender, TextChangedEventArgs e)
        {
            ; string keyword = Search.Text;

            IEnumerable<string> searchResult = from name
                in names
                where name.Contains(keyword)
                select name;
            MainListView.ItemsSource = searchResult;
        }

        private void BtnSpeak_OnClicked ( object sender, EventArgs e )
        {
            //DisplayAlert ("Speak", Search.Text, "cancel");
            if ( Search.Text != "" )
            {
                CrossTextToSpeech.Current.Speak ( Search.Text );
            }         
        }



        private async void Translate_OnClicked(object sender, EventArgs e)
        {
            if (Search.Text != "")
            {
                string url = "https://translate.yandex.net/api/v1.5/tr/translate?key=trnsl.1.1.20170322T121646Z.547f9b757cf1e75e.1bc910cdd2391946c3c417486c51071c70ec0b08&text=" + Search.Text.ToString() + "&lang=en-vi&[format=plain]&[options=lang]";
                HttpClient hpClient = new HttpClient();
                var requestMessage = await hpClient.GetAsync(url);
                string result = await requestMessage.Content.ReadAsStringAsync();
                result = result.Replace("/", "");
                result = result.Replace("<text>", "~");
                result = result.Split('~')[1];
                Result.Text = result;
            }
        }

        private byte [ ] FileToByteArray ( MediaFile file )
        {
            using (var memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                return memoryStream.ToArray();
            }
        }

        private async void Image_OnClicked(object sender, EventArgs e)
        {
            if (CrossMedia.Current.IsPickPhotoSupported)
            {
                var option = new StoreCameraMediaOptions();
                option.SaveToAlbum = true;
                var image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions());


                try
                {
                    HttpClient httpClient = new HttpClient ( );
                    httpClient.Timeout = new TimeSpan ( 1, 1, 1 );


                    MultipartFormDataContent form = new MultipartFormDataContent ( );
                    form.Add ( new StringContent ( "1ff9c5e13588957" ), "apikey" ); //Added api key in form data
                    form.Add ( new StringContent ( "eng" ), "language" );
                    var imageData = FileToByteArray ( image );
                    form.Add ( new ByteArrayContent ( imageData,0, imageData.Length ), "image", "image.jpg");


                    HttpResponseMessage response =
                        await httpClient.PostAsync ( "https://api.ocr.space/Parse/Image", form );

                    string strContent = await response.Content.ReadAsStringAsync ( );
                    strContent = strContent.Replace ( "ParsedText\":\"", "~" );
                    strContent = strContent.Replace ( "\",\"ErrorMessage", "~" );
                    string [ ] g = strContent.Split ( '~' );

                    // het


                    string Res = g [ 1 ].Replace ( "\\r\\n", "" );

                    Result.Text = Res;

                    if (Result.Text != "")
                    {
                        string url = "https://translate.yandex.net/api/v1.5/tr/translate?key=trnsl.1.1.20170322T121646Z.547f9b757cf1e75e.1bc910cdd2391946c3c417486c51071c70ec0b08&text=" + Result.Text.ToString() + "&lang=en-vi&[format=plain]&[options=lang]";
                        HttpClient hpClient = new HttpClient();
                        var requestMessage = await hpClient.GetAsync(url);
                        string result = await requestMessage.Content.ReadAsStringAsync();
                        result = result.Replace("/", "");
                        result = result.Replace("<text>", "~");
                        result = result.Split('~')[1];
                        Result.Text = result;
                    }

                    //MessageBox.Show(g[1].Replace("\\r\\n", ""));
                    //this.Close();


                }
                catch ( Exception exception )
                {
                    Result.Text = "error";
                    return;
                }
                finally
                {
                    image.Dispose (  );
                }a
            }
        }

        private void MainListView_OnItemSelected ( object sender, SelectedItemChangedEventArgs e )
        {
            Search.Text = MainListView.SelectedItem.ToString ( );
            Bamnut ( sender,e );
        }
    }
}
