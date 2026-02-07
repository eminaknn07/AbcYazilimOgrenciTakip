using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Common.Message;
using AbcYazilim.OgrenciTakip.Model.Entities.Base;
using AbcYazilim.OgrenciTakip.UI.Win.UserControls.Controls;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static DevExpress.Utils.Svg.CommonSvgImages;

namespace AbcYazilim.OgrenciTakip.UI.Win.Functions
{
    public static class GeneralFunctions
    {

        /// <summary>
        /// Tablo üzerinde odaklanılmış olan satırın benzersiz kimlik numarasını (Id) döndürür.
        /// </summary>
        /// <param name="tablo">İşlem yapılacak olan <see cref="DevExpress.XtraGrid.Views.Grid.GridView"/> nesnesi.</param>
        /// <returns>
        /// Seçili satır geçerli ise satıra ait Id değerini döndürür. 
        /// Eğer seçili bir satır yoksa kullanıcıya uyarı mesajı gösterir ve -1 döndürür.
        /// </returns>
        /// <remarks>
        /// Bu metot, özellikle düzenleme (Edit) veya silme (Delete) işlemleri başlatılmadan önce 
        /// tablodan hangi kaydın seçildiğini tespit etmek için kullanılır.
        /// </remarks>
        public static long GetRowId(this GridView tablo)
        {
            if (tablo.FocusedRowHandle > -1) return (long)tablo.GetFocusedRowCellValue("Id");
            Messages.KartSecmemeMesaji(); return -1;

        }
        /// <summary>
        /// Tablo üzerinde odaklanılmış olan satırı, belirtilen tipte bir nesne olarak döndürür.
        /// </summary>
        /// <typeparam name="T">Döndürülecek olan nesne tipi (Örn: <see cref="BaseEntity"/> veya özel bir liste modeli).</typeparam>
        /// <param name="tablo">İşlem yapılacak olan <see cref="DevExpress.XtraGrid.Views.Grid.GridView"/> nesnesi.</param>
        /// <param name="mesajVer">Seçili satır bulunamadığında kullanıcıya uyarı mesajı gösterilip gösterilmeyeceğini belirler. Varsayılan true'dur.</param>
        /// <returns>Seçili satır geçerli ise ilgili nesneyi, değilse varsayılan tipi (null) döndürür.</returns>
        /// <remarks>
        /// Bu metot, özellikle bir liste üzerinden çift tıklandığında veya butonla detay formuna gidileceğinde 
        /// tüm satır verisine nesne bazlı erişmek için kullanılır.
        /// </remarks>
        public static T GetRow<T>(this GridView tablo, bool mesajVer = true)
        {
            if (tablo.FocusedRowHandle > -1) return (T)tablo.GetRow(tablo.FocusedRowHandle);

            if (mesajVer)
                Messages.KartSecmemeMesaji();
            return default(T);
        }
        /// <summary>
        /// İki nesne arasındaki farkları property (özellik) bazında karşılaştırarak verinin değişip değişmediğini tespit eder.
        /// </summary>
        /// <typeparam name="T">Karşılaştırılacak varlık tipi.</typeparam>
        /// <param name="oldEntity">Verinin veritabanından çekilen orijinal hali.</param>
        /// <param name="currentEntity">Kullanıcı tarafından değiştirilmiş güncel hali.</param>
        /// <returns>
        /// Herhangi bir alan değişmişse <see cref="VeriDegisimYeri.Alan"/>, 
        /// hiçbir fark yoksa <see cref="VeriDegisimYeri.VeriDegisimiYok"/> döner.
        /// </returns>
        /// <remarks>
        /// Metot, Reflection kullanarak tüm özellikleri tarar. Resim gibi byte dizilerini (byte[]) ve 
        /// koleksiyon yapılarını özel olarak ele alarak performanslı bir karşılaştırma sunar.
        /// </remarks>
        private static VeriDegisimYeri veriDegisimYeriGetir<T>(T oldEntity, T currentEntity)
        {
            foreach (var prop in currentEntity.GetType().GetProperties())
            {
                if (prop.PropertyType.Namespace == "System.Collections.Generic") continue;
                var oldValue = prop.GetValue(oldEntity) ?? string.Empty;
                var currentValue = prop.GetValue(currentEntity) ?? string.Empty;

                if (prop.PropertyType == typeof(byte[]))
                {
                    if (string.IsNullOrEmpty(oldValue.ToString()))
                        oldValue = new byte[] { 0 };
                    if (string.IsNullOrEmpty(currentValue.ToString()))
                        currentValue = new byte[] { 0 };
                    if (((byte[])oldValue).Length != ((byte[])currentValue).Length)
                        return VeriDegisimYeri.Alan;
                }
                else if (!currentValue.Equals(oldValue))
                    return VeriDegisimYeri.Alan;
            }
            return VeriDegisimYeri.VeriDegisimiYok;
        }
        /// </summary>
        /// <typeparam name="T">Karşılaştırılacak varlık tipi.</typeparam>
        /// <param name="btnYeni">Yeni kayıt ekleme butonu.</param>
        /// <param name="btnKaydet">Değişiklikleri kaydetme butonu.</param>
        /// <param name="btnGeriAl">Yapılan değişiklikleri iptal etme butonu.</param>
        /// <param name="btnSil">Mevcut kaydı silme butonu.</param>
        /// <param name="oldEntity">Verinin orijinal (değişmemiş) hali.</param>
        /// <param name="currentEntity">Verinin form üzerindeki güncel hali.</param>
        /// <remarks>
        /// Eğer bir veri değişimi saptanmışsa (Alan değişmişse); Kaydet ve Geri Al butonları aktif, 
        /// Yeni ve Sil butonları pasif hale getirilir. Değişim yoksa tam tersi uygulanır.
        /// </remarks>
        public static void ButtonEnabledDurumu<T>(BarButtonItem btnYeni, BarButtonItem btnKaydet, BarButtonItem btnGeriAl, BarButtonItem btnSil, T oldEntity, T currentEntity)
        {
            var veriDegisimYeri = veriDegisimYeriGetir(oldEntity, currentEntity);
            var butonEnabledDurumu = veriDegisimYeri == VeriDegisimYeri.Alan;
            btnKaydet.Enabled = butonEnabledDurumu;
            btnGeriAl.Enabled = butonEnabledDurumu;
            btnYeni.Enabled = !butonEnabledDurumu;
            btnSil.Enabled = !butonEnabledDurumu;
        }
        /// <summary>
        /// Yapılan işlem türüne göre mevcut bir ID'yi döndürür veya zaman damgasına dayalı benzersiz yeni bir ID oluşturur.
        /// </summary>
        /// <param name="islemTuru">Gerçekleştirilen işlem türü (EntityInsert veya EntityUpdate).</param>
        /// <param name="selectedEntity">Güncelleme durumunda ID'si korunacak olan varlık.</param>
        /// <returns>
        /// İşlem güncelleme ise mevcut varlığın ID'sini, 
        /// yeni kayıt ise yyyyMMddHHmmfffRR formatında oluşturulan benzersiz sayısal değeri döndürür.
        /// </returns>
        /// <remarks>
        /// Yeni ID oluşturulurken milisaniye (salise) ve 0-99 arası rastgele bir sayı kullanılarak 
        /// aynı saniye içerisinde oluşabilecek çakışmaların (collision) önüne geçilir.
        /// </remarks>
        public static long IdOlustur(this IslemTuru islemTuru, BaseEntity selectedEntity)
        {
            string SifirEkle(string deger)
            {
                if (deger.Length == 1)
                    return "0" + deger;
                return deger;
            }
            string UcBasamakYap(string deger)
            {
                switch (deger.Length)
                {
                    case 1:
                        return "00" + deger;
                    case 2:
                        return "0" + deger;
                }
                return deger;
            }

            string Id()
            {
                var yil = DateTime.Now.Year.ToString();
                var ay = SifirEkle(DateTime.Now.Month.ToString());
                var gun = SifirEkle(DateTime.Now.Day.ToString());
                var saat = SifirEkle(DateTime.Now.Hour.ToString());
                var dakika = SifirEkle(DateTime.Now.Minute.ToString());
                var saniye = SifirEkle(DateTime.Now.Second.ToString());
                var salise = UcBasamakYap(DateTime.Now.Millisecond.ToString());
                var random = SifirEkle(new Random().Next(0, 99).ToString());

                return yil + ay + gun + saat + dakika + salise + random;
            }
            return islemTuru == IslemTuru.EntityUpdate ? selectedEntity.Id : long.Parse(Id());
        }
        /// <summary>
        /// Bir buton editörün (Master) seçim durumuna göre, ona bağlı olan diğer kontrolün (Detail) aktiflik ve veri durumunu yönetir.
        /// </summary>
        /// <param name="baseEdit">Seçim kaynağı olan ana kontrol (Örn: İl seçimi yapılan ButtonEdit).</param>
        /// <param name="prmEdit">Durumu değiştirilecek olan bağlı kontrol (Örn: İlçe seçimi yapılan ButtonEdit).</param>
        /// <remarks>
        /// Eğer ana kontrolde geçerli bir ID seçili değilse, bağlı kontrol pasif (Disabled) hale getirilir. 
        /// Ana kontrol her değiştiğinde bağlı kontrolün içeriği ve ID değeri güvenlik amacıyla temizlenir.
        /// </remarks>
        public static void ControlEnabledChange(this MyButtonEdit baseEdit, Control prmEdit)
        {
            switch (prmEdit)
            {

                case MyButtonEdit edt:
                    edt.Enabled = baseEdit.Id.HasValue && baseEdit.Id > 0;
                    edt.Id = null;
                    edt.EditValue = null;
                    break;
            }
        }
        /// <summary>
        /// Verilen kolon adı ve değer üzerinden tablo içerisinde arama yaparak, eşleşen satıra odaklanılmasını (focus) sağlar.
        /// </summary>
        /// <param name="tablo">İşlem yapılacak olan <see cref="DevExpress.XtraGrid.Views.Grid.GridView"/> nesnesi.</param>
        /// <param name="aranacakKolon">Aramanın yapılacağı kolonun adı (Örn: "Id" veya "Kod").</param>
        /// <param name="aranacakDeger">Hedeflenen satırı bulmak için kullanılacak olan değer.</param>
        /// <remarks>
        /// Metot, tablo satırlarını döngü ile tarayarak eşleşen ilk kaydın index numarasını (RowHandle) bulur 
        /// ve tablonun odaklanmış satırı olarak ayarlar. Eğer eşleşme bulunamazsa odak ilk satırda kalır.
        /// </remarks>
        public static void RowFocus(this GridView tablo, string aranacakKolon, object aranacakDeger)
        {
            var rowHandle = 0;
            for (int i = 0; i < tablo.RowCount; i++)
            {
                var bulunanDeger = tablo.GetRowCellValue(i, aranacakKolon);
                if (aranacakDeger.Equals(bulunanDeger))
                    rowHandle = i;
            }
            tablo.FocusedRowHandle = rowHandle;
        }
        /// <summary>
        /// Belirtilen satır indeksine (RowHandle) odaklanır; silme veya yer değiştirme gibi durumlarda imleç konumunu otomatik ayarlar.
        /// </summary>
        /// <param name="tablo">İşlem yapılacak olan <see cref="DevExpress.XtraGrid.Views.Grid.GridView"/> nesnesi.</param>
        /// <param name="rowHandle">Odaklanılması istenen temel satır indeksi.</param>
        /// <remarks>
        /// Metot şu mantıkla çalışır:
        /// <list type="bullet">
        /// <item><description>Eğer belirtilen indeks son satır ise, doğrudan o satıra odaklanır.</description></item>
        /// <item><description>Diğer durumlarda, kullanıcı deneyimini iyileştirmek için hedeflenen satırın bir üstündeki (-1) satıra odaklanır.</description></item>
        /// <item><description>Negatif veya sıfır değerlerde işlem yapmadan döner.</description></item>
        /// </list>
        /// </remarks>
        public static void RowFocus(this GridView tablo, int rowHandle)
        {
            if (rowHandle <= 0) return;
            if (rowHandle == tablo.RowCount - 1)
                tablo.FocusedRowHandle = rowHandle;
            else
                tablo.FocusedRowHandle = rowHandle - 1;
        }
        /// <summary>
        /// Fare sağ tuşuna tıklandığında, belirtilen açılır menüyü (PopupMenu) imlecin bulunduğu konumda gösterir.
        /// </summary>
        /// <param name="e">Fare olay verilerini içeren <see cref="MouseEventArgs"/> nesnesi.</param>
        /// <param name="sagMenu">Görüntülenecek olan <see cref="DevExpress.XtraBars.PopupMenu"/> bileşeni.</param>
        /// <remarks>
        /// Metot, sadece fare sağ tuşuna basılıp basılmadığını kontrol eder. 
        /// Koordinat hesaplamasını <see cref="Control.MousePosition"/> üzerinden yaparak menüyü tam imleç ucunda açar.
        /// </remarks>
        public static void SagMenuGoster(this MouseEventArgs e, PopupMenu sagMenu)
        {
            if (e.Button != MouseButtons.Right) return;
            sagMenu.ShowPopup(Control.MousePosition);
        }
    }
}
