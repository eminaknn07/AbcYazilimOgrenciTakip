using AbcYazilim.OgrenciTakip.Bll.Base;
using AbcYazilim.OgrenciTakip.Bll.Interfaces;
using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Data.Contexts;
using AbcYazilim.OgrenciTakip.Model.Dto;
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
    /// Okul kayıtları için gerekli iş mantığı (business logic) operasyonlarını yürüten sınıftır.
    /// </summary>
    /// <remarks>
    /// <see cref="BaseBll{T, TContext}"/> sınıfından türetilerek jenerik veri erişim yeteneklerini kazanır. 
    /// Öğrenci takip sistemi kapsamında okul tanımlamaları, bu sınıf üzerinden merkezi bir şekilde yönetilir.
    /// </remarks>
    public class OkulBll : BaseBll<Okul, OgrenciTakipContext>, IBaseGenelBll, IBaseCommonBll
    {
        public OkulBll() { }
        /// <summary>
        /// <see cref="OkulBll"/> sınıfının yeni bir örneğini, ilişkili kullanıcı arayüzü kontrolü ile başlatır.
        /// </summary>
        /// <param name="ctrl">
        /// İş mantığı süreçlerinde hata mesajlarının veya görsel uyarıların 
        /// hangi form/kontrol üzerinden kullanıcıya sunulacağını belirleyen bileşen.
        /// </param>
        /// <remarks>
        /// Gönderilen kontrol referansı, temel sınıftaki (<see cref="BaseBll{T, TContext}"/>) 
        /// merkezi mesajlaşma ve hata yakalama sistemine aktarılır.
        /// </remarks>
        public OkulBll(Control ctrl) : base(ctrl) { }
        /// <summary>
        /// Belirtilen filtre kriterine uygun olan tekil okul kaydını, ilişkili olduğu İl ve İlçe bilgileriyle birlikte getirir.
        /// </summary>
        /// <param name="filter">Sorgulanacak okulu bulmak için kullanılan filtre ifadesi.</param>
        /// <returns>
        /// İlişkili tablolarla zenginleştirilmiş okul verisini <see cref="OkulS"/> tipinde döndürür. 
        /// Kayıt bulunamazsa null döner.
        /// </returns>
        /// <remarks>
        /// Metot, veritabanı seviyesinde bir <b>JOIN</b> işlemi gerçekleştirerek İl ve İlçe isimlerini 
        /// projeksiyon (projection) yöntemiyle nesneye dahil eder. Bu sayede UI katmanında ek sorgu ihtiyacı kalmaz.
        /// </remarks>
        public BaseEntity Single(Expression<Func<Okul, bool>> filter)
        {
            return BaseSingle(filter, x => new OkulS
            {
                Id = x.Id,
                Kod = x.Kod,
                OkulAdi = x.OkulAdi,
                IlId = x.IlId,
                IlAdi = x.Il.IlAdi,
                IlceId = x.IlceId,
                IlceAdi = x.Ilce.IlceAdi,
                Aciklama = x.Aciklama,
                Durum = x.Durum,
            });
        }
        /// <summary>
        /// Belirtilen kriterlere uygun okul kayıtlarını, ilişkili İl ve İlçe adlarıyla birlikte liste olarak getirir.
        /// </summary>
        /// <param name="filter">Listelenecek okulları daraltmak için kullanılacak filtre ifadesi.</param>
        /// <returns>
        /// <see cref="OkulL"/> tipinde dönüştürülmüş, kod sırasına göre dizilmiş okul listesi döndürür.
        /// </returns>
        /// <remarks>
        /// Metot, veritabanı seviyesinde projeksiyon kullanarak sadece listede gösterilecek kolonları seçer. 
        /// Bu sayede yüksek kayıt sayılı tablolarda performans artışı sağlar.
        /// </remarks>
        public IEnumerable<BaseEntity> List(Expression<Func<Okul, bool>> filter)
        {
            return BaseList(filter, x => new OkulL
            {
                Id = x.Id,
                OkulAdi = x.OkulAdi,
                Aciklama = x.Aciklama,
                IlAdi = x.Il.IlAdi,
                IlceAdi = x.Ilce.IlceAdi,
                Kod = x.Kod,
            }).OrderBy(x => x.Kod).ToList();
        }
        public bool Insert(BaseEntity entity)
        {
            return BaseInsert(entity, x => x.Kod == entity.Kod);
        }

        public bool Update(BaseEntity oldEntity, BaseEntity currentEntity)
        {
            return BaseUpdate(oldEntity, currentEntity, x => x.Kod == currentEntity.Kod);
        }
        public bool Delete(BaseEntity entity)
        {
            return BaseDelete(entity, KartTuru.Okul);
        }

        public string YeniKodVer()
        {
            return BaseYeniKodVer(KartTuru.Okul, x => x.Kod);
        }
    }
}
