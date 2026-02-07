using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Model.Entities;
using AbcYazilim.OgrenciTakip.UI.Win.Forms.IlceForms;
using AbcYazilim.OgrenciTakip.UI.Win.Forms.IlForms;
using AbcYazilim.OgrenciTakip.UI.Win.Show;
using AbcYazilim.OgrenciTakip.UI.Win.UserControls.Controls;
using System;

namespace AbcYazilim.OgrenciTakip.UI.Win.Functions
{
    /// <summary>
    /// Uygulama genelinde liste ekranlarından veri seçme, ilişkili kayıtları bulma ve formlar arası veri aktarımı işlemlerini yöneten fonksiyonalite sınıfıdır.
    /// </summary>
    /// <remarks>
    /// <see cref="IDisposable"/> arayüzünü uygulayarak, seçim işlemleri sırasında kullanılan kaynakların 
    /// (Örn: Veritabanı bağlantıları veya geçici form nesneleri) işlem sonunda bellekten temizlenmesini garanti altına alır.
    /// </remarks>
    public class SelectFunctions : IDisposable
    {
        private MyButtonEdit _btnEdit;
        private MyButtonEdit _prmEdit;
        private KartTuru _kartTuru;
        /// <summary>
        /// Bir buton editör (ButtonEdit) üzerinden seçim sürecini başlatır ve hedef kontrolü belirler.
        /// </summary>
        /// <param name="_btnEdit">Seçim yapıldıktan sonra Id ve Ad bilgilerinin aktarılacağı <see cref="MyButtonEdit"/> nesnesi.</param>
        /// <remarks>
        /// Metot, gelen kontrolü sınıf düzeyindeki geçici bir değişkende saklar ve ardından 
        /// projenin iş kurallarına göre ilgili liste formunu açacak olan <c>SecimYap()</c> metodunu tetikler.
        /// </remarks>
        public void Sec(MyButtonEdit _btnEdit)
        {
            this._btnEdit = _btnEdit;
            SecimYap();
        }
        /// <summary>
        /// Bir parametre kontrole bağlı olarak seçim sürecini başlatır; ilişkili seçimler için hedef ve kaynak kontrolleri belirler.
        /// </summary>
        /// <param name="btnEdit">Seçim yapıldıktan sonra verinin aktarılacağı ana kontrol (Örn: İlçe seçimi).</param>
        /// <param name="prmEdit">Seçim yapılacak liste formuna filtre kriteri sağlayacak olan kaynak kontrol (Örn: Seçili olan İl).</param>
        /// <remarks>
        /// Bu aşırı yüklenmiş (overload) metot, özellikle Master-Detail ilişkisi olan seçimlerde kullanılır. 
        /// <c>SecimYap()</c> metodu içerisinde <paramref name="prmEdit"/> nesnesinin ID'si kullanılarak 
        /// açılacak liste formunun otomatik olarak filtrelenmesi sağlanır.
        /// </remarks>
        public void Sec(MyButtonEdit btnEdit, MyButtonEdit prmEdit)
        {
            _btnEdit = btnEdit;
            _prmEdit = prmEdit;
            SecimYap();
        }
        /// <summary>
        /// Odaklanılan butonun adına göre ilgili liste formunu açar ve seçilen kaydı butona geri yükler.
        /// </summary>
        /// <remarks>
        /// Metot, <c>switch-case</c> yapısı kullanarak hangi verinin (İl, İlçe vb.) seçileceğini belirler:
        /// <list type="bullet">
        /// <item><description><b>İl Seçimi:</b> Doğrudan İl listesini açar.</description></item>
        /// <item><description><b>İlçe Seçimi:</b> <c>_prmEdit</c> üzerinden gelen İl ID'sine göre filtrelenmiş ilçe listesini açar.</description></item>
        /// </list>
        /// Seçim başarılı ise dönen entity'nin Id ve Ad bilgileri ilgili kontrolün Id ve EditValue özelliklerine atanır.
        /// </remarks>
        private void SecimYap()
        {
            switch (_btnEdit.Name)
            {
                case "txtIl":
                    {
                        var entity = (Il)ShowListForms<IlListForm>.ShowDialogListForm(_kartTuru, _btnEdit.Id);
                        if (entity != null)
                        {
                            _btnEdit.Id = entity.Id;
                            _btnEdit.EditValue = entity.IlAdi;
                        }
                    }
                    break;
                case "txtIlce":
                    {
                        var entity = (Ilce)ShowListForms<IlceListForm>.ShowDialogListForm(_kartTuru, _btnEdit.Id, _prmEdit.Id, _prmEdit.Text);
                        if (entity != null)
                        {
                            _btnEdit.Id = entity.Id;
                            _btnEdit.EditValue = entity.IlceAdi;
                        }
                    }
                    break;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
