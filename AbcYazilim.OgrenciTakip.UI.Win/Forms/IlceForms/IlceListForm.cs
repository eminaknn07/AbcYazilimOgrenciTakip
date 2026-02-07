using AbcYazilim.OgrenciTakip.Bll.General;
using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Model.Entities;
using AbcYazilim.OgrenciTakip.UI.Win.Forms.BaseForms;
using AbcYazilim.OgrenciTakip.UI.Win.Functions;
using AbcYazilim.OgrenciTakip.UI.Win.Show;

namespace AbcYazilim.OgrenciTakip.UI.Win.Forms.IlceForms
{
    public partial class IlceListForm : BaseListForm
    {
        private readonly long _ilId;
        private readonly string _ilAdi;
        public IlceListForm(params object[] prm)
        {
            InitializeComponent();
            Bll = new IlceBll();
            _ilId = (long)prm[0];
            _ilAdi = prm[1].ToString();
        }
        protected override void DegiskenkeriDoldur()
        {
            Tablo = tablo;
            BaseKartTuru = KartTuru.Okul;
            Navigator=longNavigator.Navigator;
            Text = Text + $" - ( {_ilAdi} )";
        }
        protected override void Listele()
        {
            Tablo.GridControl.DataSource = ((IlceBll)Bll).List(x=>x.Durum==AktifkartlariGoster && x.IlId==_ilId);
        }
        protected override void ShowEditForm(long id)
        {
            var result = new ShowEditForms<IlceEditForm>().ShowDialogEditForm(KartTuru.Ilce,id,_ilId,_ilAdi);
            ShowEditFormDefault(result);
        }
    }
}