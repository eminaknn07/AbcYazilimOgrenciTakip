using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.UI.Win.Forms.BaseForms;
using AbcYazilim.OgrenciTakip.UI.Win.Show.Interfaces;
using System;

namespace AbcYazilim.OgrenciTakip.UI.Win.Show
{
    /// <summary>
    /// Belirtilen kart türüne ait editör formlarını jenerik olarak oluşturur ve ekranda gösterir.
    /// </summary>
    /// <typeparam name="TForm">Gösterilecek olan ve <see cref="BaseEditForm"/> sınıfından türetilmiş form tipi.</typeparam>
    /// <remarks>
    /// Bu sınıf, formların çalışma zamanında dinamik olarak örneklendirilmesini (Instantiation) sağlar. 
    /// Form açılmadan önce ID değerine göre işlem türünü (Yeni Kayıt/Güncelleme) belirler ve 
    /// form kapatıldığında liste ekranının yenilenmesi gerekip gerekmediğini bir ID değeri döndürerek bildirir.
    /// </remarks>
    public class ShowEditForms<TForm>:IBaseFormShow where TForm: BaseEditForm 
    {
        /// <summary>
        /// Belirtilen tipteki editör formunu (TForm) çalışma zamanında oluşturur ve kullanıcıya iletişim kutusu (Dialog) olarak sunar.
        /// </summary>
        /// <param name="kartTuru">İşlem yapılacak olan kartın türü (Örn: Okul, Öğrenci).</param>
        /// <param name="id">Açılacak kaydın benzersiz kimliği. Değer 0'dan büyükse güncelleme, değilse yeni kayıt modunda açılır.</param>
        /// <returns>
        /// İşlem sonunda liste ekranının yenilenmesi gerekiyorsa (kayıt/güncelleme yapıldıysa) ilgili kaydın ID'sini, 
        /// aksi takdirde 0 değerini döndürür.
        /// </returns>
        /// <remarks>
        /// Metot, formun <see cref="BaseEditForm.Yukle"/> metodunu otomatik olarak tetikleyerek 
        /// verilerin ekrana dolmasını sağlar ve kaynak yönetimi için <c>using</c> bloğunu kullanır.
        /// </remarks>
        public long ShowDialogEditForm(KartTuru kartTuru,long id)
        {
            // Yetki Kontrolü

            using (var frm =(TForm) Activator.CreateInstance(typeof(TForm)))
            {
                frm.BaseIslemTuru=id>0?IslemTuru.EntityUpdate:IslemTuru.EntityInsert;
                frm.Id=id;
                frm.Yukle();
                frm.ShowDialog();
                return frm.RefreshYapilacak?frm.Id:0;
            }           
        }
        /// <summary>
        /// Belirtilen tipteki editör formunu (TForm), verilen ek parametreleri kullanarak oluşturur ve kullanıcıya sunar.
        /// </summary>
        /// <param name="kartTuru">İşlem yapılacak olan kartın genel kategorisi.</param>
        /// <param name="id">Açılacak kaydın ID değeri (0 ise yeni kayıt).</param>
        /// <param name="prm">Hedef formun kurucu metoduna (Constructor) gönderilecek olan ek argümanlar dizisi.</param>
        /// <returns>
        /// Kayıt sonrası liste yenileme gerekiyorsa işlem yapılan kaydın ID'sini, aksi halde 0 döndürür.
        /// </returns>
        /// <remarks>
        /// <see cref="Activator.CreateInstance(Type, object[])"/> kullanılarak form nesnesi, çalışma zamanında 
        /// parametreli constructor'ı ile ayağa kaldırılır. Bu sayede bağımlı (Master-Detail) formlar arası veri transferi kolaylaşır.
        /// </remarks>
        public long ShowDialogEditForm(KartTuru kartTuru, long id,params object[] prm)
        {
            // Yetki Kontrolü

            using (var frm = (TForm)Activator.CreateInstance(typeof(TForm),prm))
            {
                frm.BaseIslemTuru = id > 0 ? IslemTuru.EntityUpdate : IslemTuru.EntityInsert;
                frm.Id = id;
                frm.Yukle();
                frm.ShowDialog();
                return frm.RefreshYapilacak ? frm.Id : 0;
            }
        }
    }
}
