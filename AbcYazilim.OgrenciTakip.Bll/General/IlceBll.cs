using AbcYazilim.OgrenciTakip.Bll.Base;
using AbcYazilim.OgrenciTakip.Bll.Interfaces;
using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Data.Contexts;
using AbcYazilim.OgrenciTakip.Model.Entities;
using AbcYazilim.OgrenciTakip.Model.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace AbcYazilim.OgrenciTakip.Bll.General
{
/// <summary>
/// İlçe kayıtları için gerekli iş mantığı (business logic) operasyonlarını yürüten sınıftır.
/// </summary>
/// <remarks>
/// Bu sınıf, ilçelerin bağlı olduğu üst varlık (İl) ile olan ilişkisini ve 
/// benzersizlik kontrollerini üst sınıftan aldığı yeteneklerle yönetir.
/// </remarks>
    public class IlceBll : BaseBll<Ilce, OgrenciTakipContext>,IBaseCommonBll
    {

        public IlceBll() { }
        /// <summary>Hata yönetimi için UI kontrolü bağlanan bir örnek başlatır.</summary>
        public IlceBll(Control ctrl):base(ctrl) { }
        /// <summary>Kriterlere uygun tekil bir ilçe kaydı getirir.</summary>
        public BaseEntity Single(Expression<Func<Ilce, bool>> filter)
        {
            return BaseSingle(filter, x => x);
        }
        /// <summary>İlçe kayıtlarını kod sırasına göre listeler.</summary>
        public IEnumerable<BaseEntity> List(Expression<Func<Ilce, bool>> filter)
        {
            return BaseList(filter, x => x).OrderBy(x => x.Kod).ToList();
        }
        /// <summary>Yeni ilçe kaydını, belirtilen filtre (genellikle İlId ve Kod) üzerinden benzersizlik kontrolü yaparak ekler.</summary>
        public bool Insert(BaseEntity entity,Expression<Func<Ilce, bool>> filter)
        {
            return BaseInsert(entity, filter);
        }
        /// <summary>Mevcut ilçe kaydını, değişen alanları ve benzersizlik kriterlerini kontrol ederek günceller.</summary>
        public bool Update(BaseEntity oldEntity, BaseEntity currentEntity,Expression<Func<Ilce,bool>>filter)
        {
          return BaseUpdate(oldEntity,currentEntity,filter);
        }
        /// <summary>Seçilen ilçeyi kullanıcı onayıyla siler.</summary>
        public bool Delete(BaseEntity entity)
        {
            return BaseDelete(entity, KartTuru.Ilce);
        }
        /// <summary>Bağlı olduğu il bazında bir sonraki boş ilçe kodunu üretir.</summary>
        public string YeniKodVer(Expression<Func<Ilce,bool>>filter)
        {
           return BaseYeniKodVer(KartTuru.Ilce,x=>x.Kod,filter);
        }
    }
}
