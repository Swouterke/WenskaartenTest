using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

namespace Wenskaarten
{
    /// <summary>
    /// Interaction logic for Wenskaart.xaml
    /// </summary>
    public partial class Wenskaart : Window
    {
        public Wenskaart()
        {
            InitializeComponent();
            //kleuren combobox vullen
            foreach (PropertyInfo info in typeof(Colors).GetProperties())
            {
                BrushConverter bc = new BrushConverter();
                SolidColorBrush deKleur = (SolidColorBrush)bc.ConvertFromString(info.Name);
                Kleur kleurke = new Kleur();
                kleurke.Borstel = deKleur;
                kleurke.Naam = info.Name;
                kleurke.Hex = deKleur.ToString();
                kleurke.Rood = deKleur.Color.R;
                kleurke.Groen = deKleur.Color.G;
                kleurke.Blauw = deKleur.Color.B;
                ComboBoxKleuren.Items.Add(kleurke);
                if (kleurke.Naam == "Black")
                    ComboBoxKleuren.SelectedItem = kleurke;
            }
        }

        //COMMANDS
        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Nieuw();
        }
        private  void Nieuw()
        {
                SolidColorBrush wit = new SolidColorBrush();
                wit.Color = Colors.White;
                Kaart.Background = wit;
                Wens.Text = "";
                ComboBoxKleuren.SelectedIndex = 7;
                ComboBoxLettertypes.SelectedIndex = 0;
                FontSize.Content = "10";
                Kaart.Children.Clear();
                Opslaan.IsEnabled = false;
                Afdrukvoorbeeld.IsEnabled = false;
        }
        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Wenskaarten |*.kaa";
                string file = "";
                if (dlg.ShowDialog() == true)
                {
                    file = dlg.FileName;
                    Nieuw();
                    using (StreamReader invoer = new StreamReader(dlg.FileName))
                    {
                        ImageBrush source = new ImageBrush();
                        string path = invoer.ReadLine();
                        source.ImageSource = new BitmapImage(new Uri(@path, UriKind.Relative));
                        Kaart.Background = source;
                        Wens.Text = invoer.ReadLine();
                        int aantalBallen = Int32.Parse(invoer.ReadLine());
                        for (var i=0; i < aantalBallen; i++)
                        {
                            Ellipse cirkel = new Ellipse();
                            Brush fill = (Brush)new BrushConverter().ConvertFromString(invoer.ReadLine());
                            cirkel.Fill = fill;
                            Canvas.SetLeft(cirkel, Double.Parse(invoer.ReadLine()));
                            Canvas.SetTop(cirkel, Double.Parse(invoer.ReadLine()));
                            cirkel.Height = 20;
                            cirkel.Width = 20;
                            cirkel.MouseMove += new MouseEventHandler(Ellipse_MouseMove);
                            Kaart.Children.Add(cirkel);
                        }
                        ComboBoxLettertypes.SelectedItem = invoer.ReadLine();
                        FontSize.Content = invoer.ReadLine();

                    }
                }
                NieuwOfNiet.Content = file;
                ImageBrush ib2 = (ImageBrush)Kaart.Background;
                string source2 = ib2.ImageSource.ToString();
                Pad.Content = source2;
            }
            catch (Exception ex)
            {
                MessageBox.Show("openen mislukt: " + ex.Message);
            }
        }
        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                DateTime time = DateTime.Now;
                dlg.FileName = time.Day.ToString() + "-" + time.Month.ToString() + "-" + time.Year.ToString() +
                    "om" + time.Hour.ToString() + "-" + time.Minute.ToString();
                dlg.DefaultExt = ".kaa";
                dlg.Filter = "Wenskaarten |*.kaa";
                if(dlg.ShowDialog() == true)
                {
                    using (StreamWriter uitvoer = new StreamWriter(dlg.FileName))
                    {
                        ImageBrush ib = (ImageBrush)Kaart.Background;
                        string source = ib.ImageSource.ToString();
                        uitvoer.WriteLine(source);
                        uitvoer.WriteLine(Wens.Text);
                        uitvoer.WriteLine(Kaart.Children.Count.ToString());
                        foreach (Ellipse bal in Kaart.Children)
                        {
                            uitvoer.WriteLine(bal.Fill);
                            uitvoer.WriteLine(Canvas.GetLeft(bal));
                            uitvoer.WriteLine(Canvas.GetTop(bal));
                        }
                        uitvoer.WriteLine(ComboBoxLettertypes.SelectedItem.ToString());
                        uitvoer.WriteLine(FontSize.Content);
                    }
                }
                NieuwOfNiet.Content = dlg.FileName;
                ImageBrush ib2 = (ImageBrush)Kaart.Background;
                string source2 = ib2.ImageSource.ToString();
                Pad.Content = source2;
            }
            catch(Exception ex)
            {
                MessageBox.Show("opslaan mislukt: " + ex.Message);
            }
        }
        private void PrintPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Afdrukvoorbeeld preview = new Afdrukvoorbeeld();
            preview.Owner = this;
            preview.AfdrukDocument = StelAfdrukSamen();
            preview.ShowDialog();
        }
        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        //PRINT & PRINTPREVIEW
        private int breedte = 500;

        private FixedDocument StelAfdrukSamen()
        {
            FixedDocument document = new FixedDocument();
            document.DocumentPaginator.PageSize = new System.Windows.Size(breedte, breedte);

            PageContent inhoud = new PageContent();
            document.Pages.Add(inhoud);

            FixedPage page = new FixedPage();
            inhoud.Child = page;

            page.Width = breedte;
            page.Height = breedte;

            Canvas canvas = new Canvas();
            canvas.Height = 350;
            canvas.Width = 500;
            ImageBrush huidig = (ImageBrush)Kaart.Background;
            ImageBrush nieuw = new ImageBrush();
            nieuw.ImageSource = huidig.ImageSource;
            canvas.Background = nieuw;
            foreach (Ellipse bal in Kaart.Children)
            {
                Ellipse cirkel = new Ellipse();
                Brush fill = bal.Fill;
                cirkel.Fill = fill;
                Canvas.SetLeft(cirkel, Canvas.GetLeft(bal));
                Canvas.SetTop(cirkel, Canvas.GetTop(bal));
                cirkel.Height = 20;
                cirkel.Width = 20;
                canvas.Children.Add(cirkel);
            }
            page.Children.Add(canvas);


            TextBlock tekst = new TextBlock();
            tekst.Text = Wens.Text;
            tekst.FontSize = Wens.FontSize;
            tekst.FontFamily = Wens.FontFamily;
            tekst.Height = 150;
            tekst.Width = 500;
            tekst.Margin = new Thickness(20, 360, 20, 20);
            tekst.TextAlignment = TextAlignment.Center;
            page.Children.Add(tekst);
            return document;
        }


        //CLOSING
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Programma afsluiten?", "Afsluiten", MessageBoxButton.YesNo, MessageBoxImage.Question
                ,MessageBoxResult.No) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        //FONTSIZE KNOPPEN
        private void Minder_Click(object sender, RoutedEventArgs e)
        {
            int size = Int32.Parse(FontSize.Content.ToString());
            size -= 1;
            if (size >= 10 && size <= 40)
                FontSize.Content = size.ToString();
        }

        private void Meer_Click(object sender, RoutedEventArgs e)
        {
            int size = Int32.Parse(FontSize.Content.ToString());
            size += 1;
            if (size >= 10 && size <= 40)
                FontSize.Content = size.ToString();
        }

        //KAARTEN
        private void Kerstkaart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Nieuw();
                ImageBrush imgB = new ImageBrush();
                imgB.ImageSource = new BitmapImage(new Uri(@"C:\Users\Cursist\source\repos\Wenskaarten\Images\kerstkaart.jpg", UriKind.Relative));
                Kaart.Background = imgB;
                Opslaan.IsEnabled = true;
                Afdrukvoorbeeld.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("probleem: " + ex.Message);
            }
        }
        private void Geboortekaart_Click(object sender, RoutedEventArgs e)
        {
            Nieuw();
            ImageBrush imgB = new ImageBrush();
            imgB.ImageSource = new BitmapImage(new Uri(@"C:\Users\Cursist\source\repos\Wenskaarten\Images\geboortekaart.jpg", UriKind.Relative));
            Kaart.Background = imgB;
            Opslaan.IsEnabled = true;
            Afdrukvoorbeeld.IsEnabled = true;
        }

        //DRAG
        private Ellipse sleepCirkel = new Ellipse();
        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            sleepCirkel = (Ellipse)sender;
            
            if ((e.LeftButton == MouseButtonState.Pressed) && (Kaart.Background != Brushes.White))
            {
                DataObject sleepKleur = new DataObject(typeof(Brush), sleepCirkel.Fill);
                DataObject sleepHoogte = new DataObject(typeof(int), sleepCirkel.Height);
                DragDrop.DoDragDrop(sleepCirkel, sleepKleur, DragDropEffects.Move);
            }
        }
        //DROP
        private void Ellipse_Drop(object sender, DragEventArgs e)
        {
            if (sender == Vuilbak)
            {
                if (sleepCirkel.Parent == Kaart)
                {
                    Kaart.Children.Remove(sleepCirkel);
                }
            }
            else
            {
                Ellipse cirkel = new Ellipse();
                Brush gesleepteKleur = (Brush)e.Data.GetData(typeof(Brush));
                cirkel.Fill = gesleepteKleur;
                cirkel.Height = 20;
                cirkel.Width = 20;
                Point positie = e.GetPosition(Kaart);
                Canvas.SetLeft(cirkel, positie.X - 10);
                Canvas.SetTop(cirkel, positie.Y - 10);
                //MessageBox.Show(positie.X.ToString());
                cirkel.MouseMove += new MouseEventHandler(Ellipse_MouseMove);
                Kaart.Children.Add(cirkel);
                if (sleepCirkel.Parent == Kaart)
                {
                    Kaart.Children.Remove(sleepCirkel);
                }
            }
        }

    }
}
