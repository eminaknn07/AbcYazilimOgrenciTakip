using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Common.Message;
using DevExpress.Export;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace AbcYazilim.OgrenciTakip.UI.Win.Functions
{
    public static class FileFunctions
    {
        /// <summary>
        /// Formun konum, boyut ve pencere durumu bilgilerini yerel bir XML dosyasına şablon olarak kaydeder.
        /// </summary>
        /// <param name="sablonAdi">Kaydedilecek şablon dosyasının benzersiz adı.</param>
        /// <param name="left">Formun ekranın sol kenarına olan uzaklığı.</param>
        /// <param name="top">Formun ekranın üst kenarına olan uzaklığı.</param>
        /// <param name="width">Formun genişliği.</param>
        /// <param name="height">Formun yüksekliği.</param>
        /// <param name="windowState">Formun pencere durumu (Normal, Maximized vb.).</param>
        /// <remarks>
        /// Metot, "Şablon Dosyaları" klasörü yoksa oluşturur. Eğer form tam ekran (Maximized) modundaysa, 
        /// genişlik ve yükseklik değerlerini -1 olarak işaretleyerek kaydeder.
        /// </remarks>
        public static void FormSablonKaydet(this string sablonAdi, int left, int top, int width, int height, FormWindowState windowState)
        {
            try
            {
                if (!Directory.Exists(Application.StartupPath + @"\Şablon Dosyaları"))
                    Directory.CreateDirectory(Application.StartupPath + @"\Şablon Dosyaları");

                var settings = new XmlWriterSettings { Indent = true };
                var writer = XmlWriter.Create(Application.StartupPath + @"\Şablon Dosyaları\" + sablonAdi + "_location.xml", settings);
                writer.WriteStartDocument();
                writer.WriteComment("ABC Yazılım Tarafından Oluşturuldu.");
                writer.WriteStartElement("Tablo");
                writer.WriteStartElement("Location");
                writer.WriteAttributeString("Left", left.ToString());
                writer.WriteAttributeString("Top", top.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("FormSize");
                if (windowState == FormWindowState.Maximized)
                {
                    writer.WriteAttributeString("Width", "-1");
                    writer.WriteAttributeString("Height", "-1");
                }
                else
                {
                    writer.WriteAttributeString("Width", width.ToString());
                    writer.WriteAttributeString("Height", height.ToString());
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                Messages.HataMesaji(ex.Message);
            }
        }
        /// <summary>
        /// Belirtilen şablon adıyla kaydedilmiş olan konum ve boyut bilgilerini XML dosyasından okuyarak ilgili forma uygular.
        /// </summary>
        /// <param name="sablonAdi">Okunacak şablon dosyasının adı.</param>
        /// <param name="frm">Ayarların uygulanacağı <see cref="DevExpress.XtraEditors.XtraForm"/> nesnesi.</param>
        /// <remarks>
        /// Metot, dosya mevcut değilse işlem yapmadan döner. XML içindeki değerleri okur ve:
        /// <list type="bullet">
        /// <item><description>Location verilerini Point olarak formun konumuna atar.</description></item>
        /// <item><description>Boyut değerleri -1 ise formu tam ekran (Maximized) yapar, değilse özel boyutları (Size) uygular.</description></item>
        /// </list>
        /// </remarks>
        public static void FormSablonYukle(this string sablonAdi, XtraForm frm)
        {
            var list = new List<string>();
            try
            {
                if (!File.Exists(Application.StartupPath + @"\Şablon Dosyaları\" + sablonAdi + "_location.xml")) return;
                var reader = XmlReader.Create(Application.StartupPath + @"\Şablon Dosyaları\" + sablonAdi + "_location.xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Location")
                    {
                        list.Add(reader.GetAttribute(0));
                        list.Add(reader.GetAttribute(1));
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "FormSize")
                    {
                        list.Add(reader.GetAttribute(0));
                        list.Add(reader.GetAttribute(1));
                    }
                }
                reader.Close();
                reader.Dispose();
            }
            catch (Exception ex)
            {
                Messages.HataMesaji(ex.Message);
            }
            if (list.Count <= 0) return;
            frm.Location = new Point(int.Parse(list[0]), int.Parse(list[1]));
            if (list[2] == "-1" && list[3] == "-1")
                frm.WindowState = FormWindowState.Maximized;
            else
                frm.Size = new Size(int.Parse(list[2]), int.Parse(list[3]));
        }

        /// <summary>
        /// DevExpress GridView tablosunun kolon dizilimi, genişliği ve görünürlük gibi yerleşim bilgilerini XML dosyasına kaydeder.
        /// </summary>
        /// <param name="tablo">Yerleşim düzeni kaydedilecek olan <see cref="DevExpress.XtraGrid.Views.Grid.GridView"/> nesnesi.</param>
        /// <param name="sablonAdi">Oluşturulacak şablon dosyasının adı.</param>
        /// <remarks>
        /// Metot, kayıt işleminden önce kolon filtrelerini temizler. Şablonlar "Şablon Dosyaları" klasörü altında saklanır. 
        /// Bu sayede kullanıcılar tabloyu kendi çalışma düzenlerine göre özelleştirebilirler.
        /// </remarks>
        public static void TabloSablonKaydet(this GridView tablo, string sablonAdi)
        {
            try
            {
                tablo.ClearColumnsFilter();
                if (!Directory.Exists(Application.StartupPath + @"\ŞablonDosyaları"))
                    Directory.CreateDirectory(Application.StartupPath + @"\Şablon Dosyaları");
                tablo.SaveLayoutToXml(Application.StartupPath + $@"\Şablon Dosyaları\{sablonAdi}.xml");
            }
            catch (Exception ex)
            {

                Messages.HataMesaji(ex.Message);
            }
        }
        /// <summary>
        /// Daha önce kaydedilmiş olan tablo yerleşim düzenini (kolon sırası, genişliği vb.) XML dosyasından okuyarak GridView'a uygular.
        /// </summary>
        /// <param name="tablo">Yerleşim düzeni uygulanacak olan <see cref="DevExpress.XtraGrid.Views.Grid.GridView"/> nesnesi.</param>
        /// <param name="sablonAdi">Yüklenecek şablon dosyasının adı.</param>
        /// <remarks>
        /// Metot, belirtilen yolda şablon dosyası olup olmadığını kontrol eder. Eğer dosya mevcutsa 
        /// <see cref="DevExpress.XtraGrid.Views.Base.BaseView.RestoreLayoutFromXml(string)"/> metodunu kullanarak 
        /// tüm görsel ayarları tabloya yansıtır.
        /// </remarks>
        public static void TabloSablonYukle(this GridView tablo, string sablonAdi)
        {
            try
            {
                if (File.Exists(Application.StartupPath + $@"\Şablon Dosyaları\{sablonAdi}.xml"))
                {
                    tablo.RestoreLayoutFromXml(Application.StartupPath + $@"\Şablon Dosyaları\{sablonAdi}.xml");
                }
            }
            catch (Exception ex)
            {
                Messages.HataMesaji(ex.Message);
            }
        }

        /// <summary>
        /// GridView üzerindeki verileri belirtilen dosya türü ve formatında dışarı aktarır.
        /// </summary>
        /// <param name="tablo">Dışarı aktarılacak olan GridView kontrolü.</param>
        /// <param name="dosyaTuru">Aktarım yapılacak dosya formatı (Excel, PDF, Word, vb.).</param>
        /// <param name="dosyaFormati">Mesaj penceresinde gösterilecek format açıklaması.</param>
        /// <param name="excelSayfaAdi">Excel aktarımlarında çalışma sayfasına (Sheet) verilecek isim.</param>
        /// <remarks>
        /// Metot şu iş akışını takip eder:
        /// <list type="number">
        /// <item><description><b>Kullanıcı Onayı:</b> İşlem öncesi kullanıcıdan onay alır.</description></item>
        /// <item><description><b>Temp Yönetimi:</b> Uygulama dizininde geçici bir "Temp" klasörü oluşturur.</description></item>
        /// <item><description><b>Benzersiz İsimlendirme:</b> Dosya çakışmalarını önlemek için <c>Guid</c> kullanarak geçici dosya ismi üretir.</description></item>
        /// <item><description><b>Özel Seçenekler:</b> Excel aktarımlarında veri tipini korumak için <c>TextExportMode.Text</c> kullanır.</description></item>
        /// </list>
        /// </remarks>
        public static void TabloDisariAktar(this GridView tablo, DosyaTuru dosyaTuru, string dosyaFormati, string excelSayfaAdi = null)
        {
            if (Messages.TabloExportMesaj(dosyaFormati) != DialogResult.Yes) return;
            if (!Directory.Exists(Application.StartupPath + @"\Temp"))
                Directory.CreateDirectory(Application.StartupPath + @"\Temp");

            var dosyaadi = Guid.NewGuid().ToString();
            var filepath = $@"{Application.StartupPath}\Temp\{dosyaadi}";

            switch (dosyaTuru)
            {
                case DosyaTuru.ExcelStandart:
                    {
                        var options = new XlsxExportOptionsEx
                        {
                            ExportType = ExportType.Default,
                            SheetName = excelSayfaAdi,
                            TextExportMode = TextExportMode.Text,
                        };
                        filepath = filepath + ".xlsx";
                        tablo.ExportToXlsx(filepath, options);
                    }
                    break;
                case DosyaTuru.ExcelFormatli:
                    {
                        var options = new XlsxExportOptionsEx
                        {
                            ExportType = ExportType.WYSIWYG,
                            SheetName = excelSayfaAdi,
                            TextExportMode = TextExportMode.Text,
                        };
                        filepath = filepath + ".xlsx";
                        tablo.ExportToXlsx(filepath, options);
                    }
                    break;
                case DosyaTuru.ExcelFormatsiz:
                    {
                        var options = new CsvExportOptionsEx
                        {
                            ExportType = ExportType.WYSIWYG,
                            TextExportMode = TextExportMode.Text,
                        };
                        filepath = filepath + ".csv";
                        tablo.ExportToCsv(filepath, options);
                    }
                    break;
                case DosyaTuru.WordDosyasi:
                    {
                        filepath = filepath + ".docx";
                        tablo.ExportToDocx(filepath);
                    }
                    break;
                case DosyaTuru.PdfDosyasi:
                    {
                        filepath = filepath + ".pdf";
                        tablo.ExportToPdf(filepath);
                    }
                    break;
                case DosyaTuru.TxtDosyasi:
                    {
                        var options = new TextExportOptions
                        {
                            TextExportMode = TextExportMode.Text,
                        };
                        filepath = filepath + ".txt";
                        tablo.ExportToText(filepath, options);
                    }
                    break;
            }

            if (!File.Exists(filepath))
            {
                Messages.HataMesaji("Dosya oluşturulamadı.");
                return;
            }
            Process.Start(filepath);
        }


    }
}
