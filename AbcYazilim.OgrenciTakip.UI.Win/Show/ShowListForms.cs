using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Model.Entities.Base;
using AbcYazilim.OgrenciTakip.UI.Win.Forms.BaseForms;
using System;
using System.Windows.Forms;

namespace AbcYazilim.OgrenciTakip.UI.Win.Show
{
    /// <summary>
    /// Belirtilen kart türüne ait liste formlarını jenerik olarak yöneten, MDI veya Dialog modunda açılmasını sağlayan sınıftır.
    /// </summary>
    /// <typeparam name="TForm">Gösterilecek olan ve <see cref="BaseListForm"/> sınıfından türetilmiş liste formu tipi.</typeparam>
    /// <remarks>
    /// Bu sınıf; ana ekranda sekmeli (MDI) formlar açmak veya seçim yapmak amacıyla açılan modal pencereleri yönetmek için kullanılır.
    /// Çalışma zamanında (Runtime) form örneklerini oluştururken varsa ek parametreleri de constructor'a iletebilir.
    /// </remarks>
    public class ShowListForms<TForm> where TForm : BaseListForm
    {
        /// <summary>
        /// Belirtilen liste formunu (TForm) çalışma zamanında oluşturur ve ana form (MDI) içerisinde görüntüler.
        /// </summary>
        /// <param name="kartTuru">Açılacak olan liste formunun temsil ettiği kart türü (Yetki kontrolü ve başlıklandırma için kullanılır).</param>
        /// <remarks>
        /// Metot şu adımları izler:
        /// <list type="number">
        /// <item><description>İlgili formun örneğini (Instance) <see cref="Activator"/> ile oluşturur.</description></item>
        /// <item><description>Formu, uygulamanın aktif olan ana penceresine (<c>MdiParent</c>) bağlar.</description></item>
        /// <item><description>Formun <c>Yukle()</c> metodunu tetikleyerek veritabanı bağlantısını kurar ve listeyi doldurur.</description></item>
        /// </list>
        /// </remarks>
        public static void ShowListForm(KartTuru kartTuru)
        {
            //Yetki Kontrolü

            var frm = (TForm)Activator.CreateInstance(typeof(TForm));
            frm.MdiParent = Form.ActiveForm;
            frm.Yukle();
            frm.Show();

        }
        /// <summary>
        /// Belirtilen liste formunu (TForm), verilen ek parametreleri kullanarak çalışma zamanında oluşturur ve MDI içerisinde görüntüler.
        /// </summary>
        /// <param name="kartTuru">Açılacak olan liste formunun kategorisi (Yetki ve kimliklendirme için).</param>
        /// <param name="prm">Hedef liste formunun yapıcı metoduna (Constructor) gönderilecek olan filtre veya ek veri argümanları.</param>
        /// <remarks>
        /// Bu metot, özellikle hiyerarşik yapılarda (Örn: Bir ile ait ilçelerin listelenmesi) 
        /// üst kayıttan gelen bilgileri liste formuna aktarmak için kullanılır. 
        /// <see cref="Activator.CreateInstance(Type, object[])"/> ile form dinamik olarak ayağa kaldırılır.
        /// </remarks>
        public static void ShowListForm(KartTuru kartTuru, params object[] prm)
        {
            //yetki kontrolü
            var frm = (TForm)Activator.CreateInstance(typeof(TForm),prm);
            frm.MdiParent = Form.ActiveForm;
            frm.Yukle();
            frm.Show();
        }
        /// <summary>
        /// Bir seçim yapmak amacıyla liste formunu modal (iletişim kutusu) olarak açar ve seçilen varlığı döndürür.
        /// </summary>
        /// <param name="kartTuru">Açılacak olan liste formunun türü.</param>
        /// <param name="seciliGelecekId">Form açıldığında tabloda otomatik olarak odaklanılması istenen kayıt ID'si.</param>
        /// <param name="prm">Liste formunun yapıcı metoduna gönderilecek ek filtreleme parametreleri.</param>
        /// <returns>Kullanıcı bir seçim yapıp 'Tamam' dediyse seçilen <see cref="BaseEntity"/> nesnesini, aksi halde null döndürür.</returns>
        /// <remarks>
        /// Metot, <c>DialogResult.OK</c> kontrolü yaparak sadece onaylanan seçimleri işleme alır. 
        /// <c>using</c> yapısı sayesinde seçim işlemi biter bitmez form nesnesi bellekten (GC) temizlenir.
        /// </remarks>
        public static BaseEntity ShowDialogListForm(KartTuru kartTuru, long? seciliGelecekId, params object[] prm)
        {
            //Yetki Kontrolü

            using (var frm = (TForm)Activator.CreateInstance(typeof(TForm), prm))
            {
                frm.SeciliGelecekId = seciliGelecekId;
                frm.Yukle();
                frm.ShowDialog();

                return frm.DialogResult == DialogResult.OK ? frm.SelectedEntity : null;
            }
        }

    }
}
