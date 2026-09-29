using Microsoft.AspNetCore.Mvc;
using KtMobileTestManager.Models;
using System.Text.Json;

namespace KtMobileTestManager.Controllers;

public class HomeController : Controller
{
    private static readonly string DataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "test_manager_db.json");
    private static readonly object FileLock = new();

    private List<TestScenario> GetInitialData()
    {
        return new List<TestScenario>
        {
            // 1. HESAPLAR
            new() { Id = "TC-ACC-01", MainModule = "Hesaplar", SubScreen = "Hesaplarım", Title = "Tüm Hesap Türlerinde Doğru Para Birimlerinin ve Sembollerin Gösterimi", AcceptanceCriteria = "Müşteriye ait tüm açık hesap türlerinde para birimleri (EUR, TRY, USD, CHF vb.) ve ilgili semboller doğru formatta ana listede kart olarak gösterilmelidir.", ExpectedResult = "Her hesap kartında para birimi sembolü ve tutar hanesi bankacılık standartlarına uygun şekilde hatasız listelenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-02", MainModule = "Hesaplar", SubScreen = "Hesaplarım", Title = "Hesap Detaylarının ve Bankacılık Bilgilerinin Görüntülenmesi", AcceptanceCriteria = "Hesap kartı seçildiğinde IBAN, hesap unvanı, hesap türü, açılış tarihi ve şube detayları eksiksiz listelenmelidir.", ExpectedResult = "Detay ekranında tüm parametreler eksiksiz açılmalı ve tek tıkla IBAN kopyalama fonksiyonu çalışmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-03", MainModule = "Hesaplar", SubScreen = "Hesaplarım", Title = "Hesap Hareketlerindeki İşlem Butonlarıyla Transfer Ekranlarına Geçiş", AcceptanceCriteria = "Hesap hareketleri dökümündeki işlem satırlarında yer alan aksiyon butonlarına basıldığında ilgili transfer ekranına otomatik yönlendirme yapılmalıdır.", ExpectedResult = "Transfer ekranı ilgili alıcı ve IBAN bilgileri form alanlarına önceden aktarılmış şekilde açılmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-04", MainModule = "Hesaplar", SubScreen = "Hesaplarım", Title = "Kullanılabilir Bakiye ve Bloke / Provizyon Tutar Ayrımı", AcceptanceCriteria = "Hesapta bekleyen provizyon veya yasal bloke varsa bakiye kartında açıkça gösterilmelidir.", ExpectedResult = "Toplam Bakiye ile harcanabilir Kullanılabilir Bakiye tutarları ayrı satırlarda net görüntülenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-05", MainModule = "Hesaplar", SubScreen = "Hesaplarım", Title = "Giden ve Gelen Transfer Sonrası Anlık Bakiye Senkronizasyonu", AcceptanceCriteria = "Hesaptan para çıktığında bakiye işlem tutarı kadar anlık düşmeli, para girişi olduğunda bakiye artarak son işlemlere yansımalıdır.", ExpectedResult = "Hesap kartındaki tutar ve hareket dökümü gecikmesiz senkronize olmalıdır.", Status = "Bekliyor" },

            new() { Id = "TC-ACC-06", MainModule = "Hesaplar", SubScreen = "Hesap Aç", Title = "Döviz Türü Seçimi ile Yeni Döviz Hesabı Açılışı", AcceptanceCriteria = "Kullanıcı döviz çeşitleri (EUR, USD, TRY, CHF) arasından seçim yaparak yeni bir döviz hesabı oluşturabilmelidir.", ExpectedResult = "Seçilen döviz cinsine uygun yeni bir IBAN üretilmeli ve yeni hesap anında aktif edilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-07", MainModule = "Hesaplar", SubScreen = "Hesap Aç", Title = "Karakter Sınırına Uygun İsimlendirme ve Hesaplarım Ekranına Anlık Yansıma", AcceptanceCriteria = "Yeni hesap açılırken belirlenen maksimum karakter sınırına uygun bir hesap adı verilebilmeli ve işlem onaylandığında yeni hesap Hesaplarım ekranında anlık listelenmelidir.", ExpectedResult = "Yeni açılan hesap, verilen isim ve sıfır bakiye ile Hesaplarım listesinde gecikmesiz görüntülenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-08", MainModule = "Hesaplar", SubScreen = "Hesap Aç", Title = "Yeni Açılan Hesabın Detay ve Açılış Tarihi Bilgilerinin Doğrulanması", AcceptanceCriteria = "Yeni açılan hesabın detay sayfasına girildiğinde sistem açılış tarihi, para birimi ve hesap numarası parametreleri doğru görüntülenmelidir.", ExpectedResult = "Hesap açılış tarihi güncel gün/saat ile tam uyuşmalı ve hesap türü doğru etiketlenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-09", MainModule = "Hesaplar", SubScreen = "Hesap Aç", Title = "Hesap Sözleşmesi ve Bilgilendirme Onayı Zorunluluğu", AcceptanceCriteria = "Yeni hesap açılışında ilgili mevzuat sözleşme kutusu onaylanmadan ilerleme yapılamamalıdır.", ExpectedResult = "Sözleşme kutucuğu seçilmediğinde 'Devam Et' butonu pasif (disabled) kalmalıdır.", Status = "Bekliyor" },

            new() { Id = "TC-ACC-10", MainModule = "Hesaplar", SubScreen = "Hesap Adı Değiştir", Title = "Karakter Sınırı Kuralına Uyarak Hesap Adının Başarıyla Değiştirilmesi", AcceptanceCriteria = "Hesap rumuz alanı en az 3, en fazla 20 karakter girilerek başarıyla güncellenebilmelidir.", ExpectedResult = "Geçerli isim kaydedildiğinde Hesaplarım ekranındaki hesap kartında yeni rumuz anında görüntülenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-11", MainModule = "Hesaplar", SubScreen = "Hesap Adı Değiştir", Title = "Maksimum Karakter Sınırı Aşımı ve Güvenlik Validasyonu", AcceptanceCriteria = "20 karakterden uzun rumuz girişi veya zararlı script/HTML karakterleri sistemce engellenmelidir.", ExpectedResult = "Karakter sayacı 20'de kilitlenmeli ve özel sembol girişinde form doğrulama hatası çıkmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-12", MainModule = "Hesaplar", SubScreen = "Hesap Adı Değiştir", Title = "Boş veya Yalnızca Boşluktan Oluşan İsim Giriş Engeli", AcceptanceCriteria = "Hesap adı kutusu tamamen boş bırakılamaz veya sadece boşluk (space) karakteri ile kaydedilemez.", ExpectedResult = "'Lütfen geçerli bir hesap adı giriniz' uyarısı çıkmalı ve kaydetme butonu kilitlenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-ACC-13", MainModule = "Hesaplar", SubScreen = "Hesap Adı Değiştir", Title = "Aynı Müşteride Mükerrer Hesap Adı Kontrolü", AcceptanceCriteria = "Kullanıcı mevcut başka bir hesabında kullandığı rumuzun aynısını farklı bir hesaba verememelidir.", ExpectedResult = "'Bu hesap rumuzu başka bir hesabınızda tanımlıdır' uyarısı verilmelidir.", Status = "Bekliyor" },

            // 2. PARA TRANSFERLERİ
            new() { Id = "TC-TRF-01", MainModule = "Para Transferleri", SubScreen = "Kendi Hesabıma Transfer", Title = "Gönderen ve Alıcı Listesinde Tüm Hesapların Doğru Bakiyeyle Listelenmesi", AcceptanceCriteria = "Kaynak ve hedef hesap açılır listelerinde müşteriye ait tüm aktif hesaplar güncel ve net bakiyeleriyle listelenmelidir.", ExpectedResult = "Her hesap satırında unvan, IBAN ve harcanabilir kullanılabilir bakiye eksiksiz görünmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-02", MainModule = "Para Transferleri", SubScreen = "Kendi Hesabıma Transfer", Title = "Kaynak ve Hedef Hesabın Aynı Seçilmesini Engelleme", AcceptanceCriteria = "Kaynak hesap seçildikten sonra aynı hesap hedef hesap listesinde seçilememelidir.", ExpectedResult = "Seçilen kaynak hesap hedef listeden gizlenmeli ya da pasif duruma getirilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-03", MainModule = "Para Transferleri", SubScreen = "Kendi Hesabıma Transfer", Title = "Tutar Sıfırdan Büyük Olmalı ve Bakiye Aşımı Engeli", AcceptanceCriteria = "Transfer tutarı kesinlikle 0'dan büyük (> 0) olmalı ve kaynak hesaptaki kullanılabilir bakiyeyi aşamaz.", ExpectedResult = "0 veya bakiye üstü tutar girildiğinde 'Yetersiz kullanılabilir bakiye' uyarısı basılmalı ve onay engellenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-04", MainModule = "Para Transferleri", SubScreen = "Kendi Hesabıma Transfer", Title = "Farklı Para Birimleri Arası Otomatik Kur Hesaplaması", AcceptanceCriteria = "EUR hesabından TRY veya USD hesabına aktarım yapılırken anlık kur ve hedef hesaba geçecek net tutar hesaplanmalıdır.", ExpectedResult = "Uygulanan kur oranı ve hedef tutar açıkça belirtilerek kullanıcı onayı istenmelidir.", Status = "Bekliyor" },

            new() { Id = "TC-TRF-05", MainModule = "Para Transferleri", SubScreen = "SEPA Transfer", Title = "SEPA IBAN Formatı ve Kontrol Hanesi (MOD-97) Doğrulaması", AcceptanceCriteria = "Girilen alıcı IBAN'ı geçerli bir Avrupa SEPA ülkesine ait olmalı ve MOD-97 matematiksel algoritmasından geçmelidir.", ExpectedResult = "Geçersiz veya hatalı kontrol haneli IBAN girildiğinde 'Geçersiz SEPA IBAN' uyarısı verilmeli ve çerçeve kırmızı olmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-06", MainModule = "Para Transferleri", SubScreen = "SEPA Transfer", Title = "SEPA Transferinde Sıfır Tutar ve Bakiye Aşımı Kontrolü", AcceptanceCriteria = "Transfer tutarı 0'dan büyük olmalı ve kaynak hesabın kullanılabilir bakiyesinden 1 Cent dahi fazla olamaz.", ExpectedResult = "Bakiye üstü tutarlarda 'Kullanılabilir bakiye yetersizdir' uyarısı verilerek devam butonu pasif kalmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-07", MainModule = "Para Transferleri", SubScreen = "SEPA Transfer", Title = "Günlük Maksimum SEPA Transfer Limiti Kontrolü", AcceptanceCriteria = "Kullanıcı günlük tanımlı SEPA limitini (örn. 2.000 EUR) aşan tutar girdiğinde işlem engellenmelidir.", ExpectedResult = "Ekranda limit aşım uyarısı çıkmalı ve onay butonu kilitlenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-08", MainModule = "Para Transferleri", SubScreen = "SEPA Transfer", Title = "Alıcı Unvanı ve Açıklama Alanı 140 Karakter Kuralı", AcceptanceCriteria = "Alıcı unvanı en az 2 karakter olmalı; SEPA standartları gereği açıklama alanına en fazla 140 karakter yazılabilmelidir.", ExpectedResult = "140 karakterden fazlası yazılamamalı ve unvan boş bırakıldığında ilerleme engellenmelidir.", Status = "Bekliyor" },

            new() { Id = "TC-TRF-09", MainModule = "Para Transferleri", SubScreen = "Kuveyt Türk Direkt", Title = "Yalnızca Türkiye (TR) IBAN ve Kuveyt Türk Hesap Kabulü", AcceptanceCriteria = "Kuveyt Türk Direkt transferinde yalnızca Türkiye (TR) ile başlayan IBAN veya KT Türkiye şube/hesap no girilebilir.", ExpectedResult = "TR dışı (DE, FR vb.) bir IBAN girildiğinde 'Bu kanaldan sadece Kuveyt Türk Türkiye hesaplarına gönderim yapılabilir' uyarısı dönmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-10", MainModule = "Para Transferleri", SubScreen = "Kuveyt Türk Direkt", Title = "Tutar Sıfırdan Büyük Olmalı ve Hesap Bakiyesinden Fazla Olamaz", AcceptanceCriteria = "Transfer tutarı 0'dan büyük olmalı ve seçilen cari hesabın bakiyesini kesinlikle aşamaz.", ExpectedResult = "Bakiye üzeri veya negatif tutar girişlerinde sistem onay butonunu açmamalı ve hata mesajı vermelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-11", MainModule = "Para Transferleri", SubScreen = "Kuveyt Türk Direkt", Title = "Alıcı Unvanının Maskeli Doğrulanması ve Masrafsız Gönderim", AcceptanceCriteria = "Hesap no girildiğinde servis alıcı unvanını maskeli (örn: A**** B****) getirmeli ve masraf 0.00 EUR olarak sunulmalıdır.", ExpectedResult = "Onay ekranında masrafsız hızlı transfer özeti doğrulanmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-12", MainModule = "Para Transferleri", SubScreen = "Kuveyt Türk Direkt", Title = "Geçersiz veya Kapatılmış Hesap Numarası Sorgusu", AcceptanceCriteria = "Sistemde kaydı bulunmayan veya kapatılmış bir KT Türkiye hesabı girildiğinde işlem engellenmelidir.", ExpectedResult = "'Girilen alıcı hesap bilgisi aktif değildir' uyarısı dönmelidir.", Status = "Bekliyor" },

            new() { Id = "TC-TRF-13", MainModule = "Para Transferleri", SubScreen = "Yurt Dışı Transfer", Title = "SWIFT / BIC Kodu Format ve Banka Doğrulaması", AcceptanceCriteria = "Uluslararası transferlerde 8 veya 11 haneli banka BIC kodu geçerlilik kontrolünden geçmelidir.", ExpectedResult = "Hatalı BIC girildiğinde 'Muhabir banka bulunamadı' hatası basılmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-14", MainModule = "Para Transferleri", SubScreen = "Yurt Dışı Transfer", Title = "Tutar Sıfırdan Büyük Olmalı ve Toplam Masraf Bakiye Kontrolü", AcceptanceCriteria = "Tutar > 0 olmalı; transfer tutarı ile banka muhabir masrafı toplamı kaynak hesap bakiyesini aşamaz.", ExpectedResult = "Tutar + masraf bakiyeyi aştığında 'Yetersiz bakiye' uyarısı verilmeli ve işlem kilitlenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-15", MainModule = "Para Transferleri", SubScreen = "Yurt Dışı Transfer", Title = "Masraf Türü Seçimi (OUR / BEN / SHA)", AcceptanceCriteria = "Kullanıcı transfer masrafının kimin tarafından ödeneceğini (Gönderen, Alıcı, Ortak) seçebilmelidir.", ExpectedResult = "Seçilen masraf tipine göre hesaptan tahsil edilecek tutar özette güncellenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-16", MainModule = "Para Transferleri", SubScreen = "Yurt Dışı Transfer", Title = "Yaptırımlı / Ambargolu Ülkelere Para Çıkışı Engeli", AcceptanceCriteria = "Uluslararası regülasyonlarca yasaklı veya yaptırım uygulanan ülkelere para transferi engellenmelidir.", ExpectedResult = "'Bu ülkeye para transferi gerçekleştirilememektedir' güvenlik mesajı basılmalıdır.", Status = "Bekliyor" },

            new() { Id = "TC-TRF-17", MainModule = "Para Transferleri", SubScreen = "Kayıtlı İşlemler", Title = "Kayıtlı Alıcı Seçimiyle Formun Otomatik Dolması", AcceptanceCriteria = "Kayıtlı işlemlerden bir alıcı seçildiğinde alıcı adı ve IBAN alanları otomatik doldurulmalıdır.", ExpectedResult = "Kullanıcı sadece tutar girerek transfer adımını hızlıca tamamlayabilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-18", MainModule = "Para Transferleri", SubScreen = "Kayıtlı İşlemler", Title = "Kayıtlı Alıcıyı Listeden Silme", AcceptanceCriteria = "Kullanıcı kayıtlı bir kişiyi onay alarak listeden başarıyla silebilmelidir.", ExpectedResult = "Silme işleminden sonra kayıtlı liste anında güncellenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-19", MainModule = "Para Transferleri", SubScreen = "Kayıtlı İşlemler", Title = "Kayıtlı Alıcı Rumuzunu Düzenleme", AcceptanceCriteria = "Kullanıcı kayıtlı alıcının ismini veya açıklamasını düzenleyip kaydedebilmelidir.", ExpectedResult = "Yeni rumuz hızlı transfer ekranında anında görünür hale gelmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-20", MainModule = "Para Transferleri", SubScreen = "Kayıtlı İşlemler", Title = "Maksimum Kayıtlı Alıcı Sınırı Kontrolü", AcceptanceCriteria = "Kayıtlı alıcı sayısı 50 kişiye ulaştığında yeni kayıt ekleme uyarılmalıdır.", ExpectedResult = "'Kayıtlı alıcı sınırına ulaştınız' uyarısı gösterilmelidir.", Status = "Bekliyor" },

            new() { Id = "TC-TRF-21", MainModule = "Para Transferleri", SubScreen = "Talimatlar", Title = "Düzenli Para Transferi Talimatı Oluşturma", AcceptanceCriteria = "Kullanıcı her ayın belirli bir gününde tekrarlanmak üzere otomatik kira/fatura talimatı girebilmelidir.", ExpectedResult = "Talimat listesinde bir sonraki çalışma tarihi ve periyot bilgisi doğru listelenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-22", MainModule = "Para Transferleri", SubScreen = "Talimatlar", Title = "Talimat Tutarında Sıfırdan Büyük Olma ve Pozitif Tutar Zorunluluğu", AcceptanceCriteria = "Talimat tutarı 0'dan büyük pozitif bir değer olmalıdır.", ExpectedResult = "0 EUR veya eksi tutarlı düzenli talimat oluşturulması engellenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-23", MainModule = "Para Transferleri", SubScreen = "Talimatlar", Title = "İleri Tarihli Düzenli Transfer İptali", AcceptanceCriteria = "Vadesi henüz gelmemiş ileri tarihli talimat tek tıkla iptal edilebilmelidir.", ExpectedResult = "Talimat durumu 'İptal Edildi'ye dönmeli ve hesap işlem kuyruğundan silinmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-TRF-24", MainModule = "Para Transferleri", SubScreen = "Talimatlar", Title = "Hafta Sonuna Gelen Talimatların İlk İş Gününe Ötelenmesi", AcceptanceCriteria = "Talimat günü resmi tatile geldiğinde ödemenin ilk iş günü yapılması kuralı doğrulanmalıdır.", ExpectedResult = "Sonraki işlem tarihi takvime göre otomatik ilk iş gününe set edilmelidir.", Status = "Bekliyor" },

            // 3. DÖVİZ VE ALTIN İŞLEMLERİ
            new() { Id = "TC-FX-01", MainModule = "Döviz ve Altın İşlemleri", SubScreen = "Döviz ve Altın Al Sat", Title = "Alış ve Satış Ekranlarında Kurların Liste ile Birebir Tutarlılığı", AcceptanceCriteria = "Alış ve satış pencerelerinde teklif edilen kur değerleri, ana kur listesindeki canlı verilerle birebir uyuşmalıdır.", ExpectedResult = "Kur değişimlerinde ekrandaki değerler gecikmesiz senkronize olmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-FX-02", MainModule = "Döviz ve Altın İşlemleri", SubScreen = "Döviz ve Altın Al Sat", Title = "Çıkan ve Giren Hesap Bakiyelerinin Doğruluğu ve Hareketlere Yansıması", AcceptanceCriteria = "İşlem tamamlandığında kaynak hesaptan çıkan tutar ile hedef hesaba giren döviz/altın tutarı kur hesabıyla tam tutarlı olmalı ve hesap hareketlerine anında işlenmelidir.", ExpectedResult = "Her iki hesabın bakiyesi hatasız güncellenmeli ve hesap ekstrelerinde işlem dökümü görüntülenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-FX-03", MainModule = "Döviz ve Altın İşlemleri", SubScreen = "Döviz ve Altın Al Sat", Title = "Anlık Kur Süre Aşımı (Timeout) ve Fiyat Yenileme Kontrolü", AcceptanceCriteria = "Ekranda sabitlenen işlem kuru 30 saniye geçerli kalmalı; süre bitiminde kullanıcı onayıyla güncellenmelidir.", ExpectedResult = "Süre dolduğunda 'Kurlar yenilendi, lütfen teyit ediniz' uyarısı basılmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-FX-04", MainModule = "Döviz ve Altın İşlemleri", SubScreen = "Döviz ve Altın Al Sat", Title = "Alım Tutarında Sıfırdan Büyük Olma ve Bakiye Aşımı Kontrolü", AcceptanceCriteria = "Alınacak tutarın ana para karşılığı kesinlikle 0'dan büyük olmalı ve kaynak hesap bakiyesini kesinlikle aşamaz.", ExpectedResult = "Bakiyeden yüksek tutar girişlerinde sistem 'Yetersiz bakiye' uyarısı vererek onay butonunu kilitlemelidir.", Status = "Bekliyor" },

            new() { Id = "TC-FX-05", MainModule = "Döviz ve Altın İşlemleri", SubScreen = "Altın Alış Satış Talimatları", Title = "Verilen Altın Talimatlarının Başarıyla Listelenmesi", AcceptanceCriteria = "Müşterinin tanımladığı tüm aktif ve geçmiş altın alım/satım emirleri ana talimat listesinde eksiksiz listelenmelidir.", ExpectedResult = "Hedef kur, talimat tutarı ve vade bilgisi talimat kartında doğru gösterilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-FX-06", MainModule = "Döviz ve Altın İşlemleri", SubScreen = "Altın Alış Satış Talimatları", Title = "Talimatların Statülere Göre Filtrelenebilmesi (Bekliyor, Gerçekleşti, İptal, Başarısız)", AcceptanceCriteria = "Kullanıcı talimat listesini 'Bekliyor', 'Gerçekleşti', 'İptal Edildi' ve 'Başarısız' statü filtreleriyle görüntüleyebilmelidir.", ExpectedResult = "Seçilen statüye ait olmayan emirler listeden gizlenmeli, filtreleme anlık çalışmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-FX-07", MainModule = "Döviz ve Altın İşlemleri", SubScreen = "Altın Alış Satış Talimatları", Title = "Talimatların Oluşturulma Tarihine Göre Sıralanması", AcceptanceCriteria = "Tüm talimat kayıtları en yeniden en eskiye veya seçime göre kronolojik sıralanabilmelidir.", ExpectedResult = "Tarih sıralaması yapıldığında kayıtların işlem zamanı doğruluğu korunmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-FX-08", MainModule = "Döviz ve Altın İşlemleri", SubScreen = "Altın Alış Satış Talimatları", Title = "Bekleyen Statüdeki Altın Talimatının İptal Edilmesi", AcceptanceCriteria = "Henüz gerçekleşmemiş 'Bekliyor' durumundaki bir talimat kullanıcı tarafından iptal edilebilmelidir.", ExpectedResult = "Talimat statüsü anında 'İptal Edildi'ye dönmeli ve hesap üzerindeki bloke kaldırılmalıdır.", Status = "Bekliyor" },

            // 4. KARTLAR
            new() { Id = "TC-CRD-01", MainModule = "Kartlar", SubScreen = "Debit Kart", Title = "Geçici Kart Kilitleme (Freeze) Durumu", AcceptanceCriteria = "Kullanıcı kartını dondurduğunda kart tüm fiziki POS ve e-ticaret harcamalarına kapanmalıdır.", ExpectedResult = "Kart durumu anında 'Kilitli'ye dönmeli ve harcama denemesinde provizyon reddedilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-02", MainModule = "Kartlar", SubScreen = "Debit Kart", Title = "İnternet Alışveriş Yetkisi Açma / Kapama", AcceptanceCriteria = "Online alışveriş yetkisi kapatıldığında kartın e-ticaret işlemlerine kapandığı doğrulanmalıdır.", ExpectedResult = "Ayar anında veri tabanına işlenmeli ve anlık toast bilgi mesajı verilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-03", MainModule = "Kartlar", SubScreen = "Debit Kart", Title = "Temassız Ödeme Özelliğini Kapatma", AcceptanceCriteria = "Kullanıcı kartın temassız işlem özelliğini açıp kapatabilmelidir.", ExpectedResult = "Ayar kapatıldığında POS cihazlarında şifresiz temassız geçiş engellenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-04", MainModule = "Kartlar", SubScreen = "Debit Kart", Title = "Kart PIN / Şifre Belirleme Kuralı", AcceptanceCriteria = "Yeni şifre belirlenirken ardışık sayılar (1234) veya doğum yılı gibi basit kombinasyonlar engellenmelidir.", ExpectedResult = "'Daha güvenli bir şifre seçiniz' uyarısı verilmeli ve yeni PIN onaylanmamalıdır.", Status = "Bekliyor" },

            new() { Id = "TC-CRD-05", MainModule = "Kartlar", SubScreen = "Debit Kart Başvurusu", Title = "Teslimat Adresi Doğrulama ve Onay", AcceptanceCriteria = "Müşterinin sistemde kayıtlı ikamet adresi listelenmeli veya yeni adres girişine izin verilmelidir.", ExpectedResult = "Posta kodu ve şehir formatı doğrulandıktan sonra başvuru takip no üretilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-06", MainModule = "Kartlar", SubScreen = "Debit Kart Başvurusu", Title = "Mevcut Aktif Kart Sayısı Kontrolü", AcceptanceCriteria = "Aynı hesaba bağlı aktif debit kartı bulunan kullanıcı ikinci bir asıl kart başvurusu yapamamalıdır.", ExpectedResult = "'Bu hesabınıza bağlı aktif bir kartınız bulunmaktadır' uyarısı gösterilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-07", MainModule = "Kartlar", SubScreen = "Debit Kart Başvurusu", Title = "Kart Üzerinde Basılacak İsim Formatı", AcceptanceCriteria = "Kart üzerine yazılacak ad soyad alanı maksimum 26 karakter olmalı ve kimlik adıyla uyuşmalıdır.", ExpectedResult = "Uzun unvanlar standart kısaltma kurallarına göre kart önizlemesinde gösterilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-08", MainModule = "Kartlar", SubScreen = "Debit Kart Başvurusu", Title = "Kurye Teslimat Takip Numarası Oluşturma", AcceptanceCriteria = "Başvuru tamamlandığında kurye takip kodu ve tahmini teslim tarihi (3-5 iş günü) ekranda çıkmalıdır.", ExpectedResult = "Başvuru başarıyla onaylanmalı ve SMS ile bilgilendirme gitmelidir.", Status = "Bekliyor" },

            new() { Id = "TC-CRD-09", MainModule = "Kartlar", SubScreen = "Jetzz Card", Title = "Jetzz Card Günlük Harcama Limiti Değiştirme", AcceptanceCriteria = "Kullanıcı günlük sanal/internet alışveriş limitini belirleyip güncelleyebilmelidir.", ExpectedResult = "Yeni limit tutarı kaydedildikten sonra kart ekranında anında yansımalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-10", MainModule = "Kartlar", SubScreen = "Jetzz Card", Title = "Yurt Dışı Kullanım Yetkisi Açma / Kapatma", AcceptanceCriteria = "Kartın yurt dışı fiziki ve e-ticaret harcamalarına izin verilip verilmeyeceği seçilebilmelidir.", ExpectedResult = "Kapalı konuma getirildiğinde yabancı menşeili POS işlemlerinde kart otomatik reddedilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-11", MainModule = "Kartlar", SubScreen = "Jetzz Card", Title = "Hesap Özeti / Ekstre İndirme", AcceptanceCriteria = "Jetzz Card dönem içi harcamaları ve son kesilen ekstre tek tıkla görüntülenebilmelidir.", ExpectedResult = "Güncel borç, asgari ödeme ve son ödeme tarihi hatasız listelenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-12", MainModule = "Kartlar", SubScreen = "Jetzz Card", Title = "Kayıp / Çalıntı Bildirimi ve Anında İptal", AcceptanceCriteria = "Kullanıcı acil kayıp bildirdiğinde kart derhal kalıcı olarak kapatılmalı ve yeni kart siparişi sorulmalıdır.", ExpectedResult = "Kart statüsü 'İptal / Çalıntı'ya çekilmeli ve tüm işlemlere kapatılmalıdır.", Status = "Bekliyor" },

            new() { Id = "TC-CRD-13", MainModule = "Kartlar", SubScreen = "Jetzz Card Başvurusu", Title = "Müşteri Kredi Riski ve Uygunluk Kontrolü", AcceptanceCriteria = "Kredi kartı özelliği taşıyan Jetzz Card için risk puanı ve Schufa skoru kontrolü yapılmalıdır.", ExpectedResult = "Kriteri sağlamayan başvurularda 'Şubeye başvurunuz' yönlendirmesi yapılmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-14", MainModule = "Kartlar", SubScreen = "Jetzz Card Başvurusu", Title = "Aylık Gelir Beyanı ve Limit Talebi", AcceptanceCriteria = "Kullanıcı aylık net gelirini beyan etmeli ve talep edilen limit gelirinin 3 katını aşamamalıdır.", ExpectedResult = "Gelir sınırını aşan limit taleplerinde sistem otomatik olarak maksimum onaylanabilir tutarı önermelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-15", MainModule = "Kartlar", SubScreen = "Jetzz Card Başvurusu", Title = "Otomatik Ödeme Talimatı Tanımlama Zorunluluğu", AcceptanceCriteria = "Başvuru adımlarında vadesiz cari hesaptan borcun tamamı veya asgari tutarı için ödeme talimatı seçilmelidir.", ExpectedResult = "Talimat seçilmeden başvuru sözleşme onay ekranına ilerlememelidir.", Status = "Bekliyor" },
            new() { Id = "TC-CRD-16", MainModule = "Kartlar", SubScreen = "Jetzz Card Başvurusu", Title = "Dijital Kredi Sözleşmesi SMS Onayı", AcceptanceCriteria = "Kredi kartı sözleşmesi müşterinin kayıtlı cep telefonuna gönderilen 6 haneli OTP ile onaylanmalıdır.", ExpectedResult = "Hatalı SMS kodu girildiğinde işlem askıya alınmalı, doğru kodla başvuru başarıyla tamamlanmalıdır.", Status = "Bekliyor" },

            // 5. AYARLAR
            new() { Id = "TC-SET-01", MainModule = "Ayarlar", SubScreen = "Dil Seçenekleri", Title = "Seçilen Dile (Almanca, İngilizce, Türkçe) Göre Tüm Menü ve Butonların Uyumlu Olması", AcceptanceCriteria = "Kullanıcı Almanca, İngilizce veya Türkçe dillerinden birini seçtiğinde tüm menüler, butonlar, uyarılar ve metinler eksiksiz o dile geçmelidir.", ExpectedResult = "Seçilen dile ait dil paketi anında yüklenmeli, hiçbir alanda çevrilmemiş etiket kalmamalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-SET-02", MainModule = "Ayarlar", SubScreen = "Dil Seçenekleri", Title = "Uygulamadan Çıkış Yapılıp Geri Girildiğinde Seçilen Dilin Kalıcı Kalması", AcceptanceCriteria = "Kullanıcı dil tercihini değiştirdikten sonra uygulamadan tamamen çıkış yapıp tekrar giriş yaptığında en son seçilen dil aktif kalmalıdır.", ExpectedResult = "Dil tercihi kullanıcı oturum profilinde kalıcı saklanmalı, varsayılan dile geri dönmemelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SET-03", MainModule = "Ayarlar", SubScreen = "Dil Seçenekleri", Title = "Cihazın Varsayılan Dilini Otomatik Algılama", AcceptanceCriteria = "Uygulama ilk kez yüklendiğinde kullanıcının telefon işletim sistemi dilini otomatik tanıyarak açılış yapmalıdır.", ExpectedResult = "Telefon dili Almanca ise uygulama doğrudan Deutsch olarak başlatılmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-SET-04", MainModule = "Ayarlar", SubScreen = "Dil Seçenekleri", Title = "Dil Değişimi Sonrası Tarih ve Para Birimi Formatı", AcceptanceCriteria = "Dil değiştiğinde tarih formatı seçilen bölgeye göre (DD.MM.YYYY veya MM/DD/YYYY) ayarlanmalıdır.", ExpectedResult = "Tarih ve saat görünümleri seçilen yerel kültüre göre biçimlendirilmelidir.", Status = "Bekliyor" },

            new() { Id = "TC-SET-05", MainModule = "Ayarlar", SubScreen = "Bildirimler", Title = "İşlem Bildirimi (Push Notification) İzin Kontrolü", AcceptanceCriteria = "Para transferi ve güvenlik bildirimleri açık/kapalı konuma getirilebilmelidir.", ExpectedResult = "İzin kapatıldığında cihaza anlık işlem bildirimi gönderilmemelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SET-06", MainModule = "Ayarlar", SubScreen = "Bildirimler", Title = "Pazarlama ve Kampanya Bildirim İzin Ayrımı", AcceptanceCriteria = "Güvenlik bildirimleri zorunlu tutulurken kampanya bildirimleri kullanıcının tercihine bırakılmalıdır.", ExpectedResult = "Müşteri pazarlama izinlerini kapatsa dahi transfer güvenlik SMS/bildirimlerini almaya devam etmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SET-07", MainModule = "Ayarlar", SubScreen = "Bildirimler", Title = "Hesap Hareketleri Tutar Eşiği Bildirimi", AcceptanceCriteria = "Kullanıcı sadece belirlediği tutarın (örn: 100 EUR) üzerindeki harcamalarda bildirim gelmesini ayarlayabilmelidir.", ExpectedResult = "Eşik tutar altındaki küçük harcamalarda anlık bildirim atılmamalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-SET-08", MainModule = "Ayarlar", SubScreen = "Bildirimler", Title = "E-Posta Ekstre Bildirim Tercihi", AcceptanceCriteria = "Aylık hesap özeti çıktığının e-posta ile iletilmesi seçeneği aktif/pasif yapılabilmelidir.", ExpectedResult = "Seçim anında müşteri iletişim izinleri tablosunda güncellenmelidir.", Status = "Bekliyor" },

            new() { Id = "TC-SET-09", MainModule = "Ayarlar", SubScreen = "Face ID / Touch ID", Title = "Biyometrik Giriş Entegrasyon Validasyonu", AcceptanceCriteria = "Biyometrik doğrulama aktifken şifre girmeden parmak izi/yüz tanıma ile login olunmalıdır.", ExpectedResult = "Hatalı 3 biyometrik denemeden sonra sistem güvenli şifre ekranına yönlendirmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SET-10", MainModule = "Ayarlar", SubScreen = "Face ID / Touch ID", Title = "Cihaz Donanım Desteği Kontrolü", AcceptanceCriteria = "Biyometrik donanımı (sensörü) olmayan cihazlarda bu menü pasif veya gizli olmalıdır.", ExpectedResult = "'Cihazınız biyometrik doğrulamayı desteklememektedir' uyarısı verilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SET-11", MainModule = "Ayarlar", SubScreen = "Face ID / Touch ID", Title = "Yüz Değişikliğinde Güvenlik Blokajı", AcceptanceCriteria = "Cihaza yeni bir parmak izi veya yüz eklendiğinde uygulama güvenliği için biyometrik giriş otomatik kapatılmalıdır.", ExpectedResult = "Kullanıcıdan ana şifresini girerek biyometriği yeniden aktive etmesi istenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SET-12", MainModule = "Ayarlar", SubScreen = "Face ID / Touch ID", Title = "Hızlı İşlem Onaylarında Biyometri Kullanımı", AcceptanceCriteria = "Giriş haricinde para transferi onaylarında da biyometrik doğrulama kullanılabilmelidir.", ExpectedResult = "Onay butonuna basıldığında Face ID sensörü doğrudan devreye girmelidir.", Status = "Bekliyor" },

            new() { Id = "TC-SET-13", MainModule = "Ayarlar", SubScreen = "Profilim", Title = "İletişim Bilgileri (E-posta ve GSM) Maskeli Görüntüleme", AcceptanceCriteria = "Müşterinin cep telefonu ve e-posta adresi güvenli maskeli biçimde gösterilmelidir.", ExpectedResult = "Örn: e***@gmail.com ve +49 17* *** **90 şeklinde güvenli maskeleme olmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-SET-14", MainModule = "Ayarlar", SubScreen = "Profilim", Title = "Kayıtlı İkamet Adresi Görüntüleme", AcceptanceCriteria = "Banka sisteminde kayıtlı resmi adres sokak, kapı no, posta kodu ve şehir olarak doğru gösterilmelidir.", ExpectedResult = "Veri tabanındaki müşteri adres kaydı ile arayüzdeki metin birebir örtüşmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SET-15", MainModule = "Ayarlar", SubScreen = "Profilim", Title = "Vergi Numarası (Steuer-ID) Alan Doğrulaması", AcceptanceCriteria = "Almanya vergi kimlik numarası 11 haneli nümerik formatta kontrol edilmelidir.", ExpectedResult = "Hatalı hane veya harf girildiğinde vergi no alanı kaydedilememelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SET-16", MainModule = "Ayarlar", SubScreen = "Profilim", Title = "Profil Fotoğrafı Yükleme ve Boyut Kısıtı", AcceptanceCriteria = "Kullanıcı profil resmi seçebilmeli; maksimum 5 MB ve JPG/PNG formatı denetlenmelidir.", ExpectedResult = "5 MB üzeri dosyalarda 'Dosya boyutu çok büyük' uyarısı çıkmalıdır.", Status = "Bekliyor" },

            // 6. ARKADAŞINA ÖNER
            new() { Id = "TC-REF-01", MainModule = "Arkadaşına Öner", SubScreen = "Arkadaşına Öner", Title = "Davet Kodu Üretimi ve Pano Paylaşımı", AcceptanceCriteria = "Müşteriye özel 8 karakterli benzersiz referans kodu üretilmeli ve tek tıkla kopyalanabilmelidir.", ExpectedResult = "Kopyalama butonuna basıldığında 'Kod kopyalandı' toast uyarısı çıkmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-REF-02", MainModule = "Arkadaşına Öner", SubScreen = "Arkadaşına Öner", Title = "Sosyal Medya ve Mesajlaşma Kanallarıyla Doğrudan Paylaşım", AcceptanceCriteria = "WhatsApp, E-posta ve SMS paylaşım butonları tıklandığında önceden tanımlı davet metni açılmalıdır.", ExpectedResult = "Cihazın yerel paylaşım menüsü açılarak referans linki metne otomatik eklenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-REF-03", MainModule = "Arkadaşına Öner", SubScreen = "Arkadaşına Öner", Title = "Başarılı Referans ve Kazanılan Ödül Takip Sayacı", AcceptanceCriteria = "Davet koduyla hesap açan arkadaş sayısı ve kazanılan bonus/hediye tutarı sayaç olarak listelenmelidir.", ExpectedResult = "Her yeni başarılı üyelikte davet eden müşterinin ödül bakiyesi anlık güncellenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-REF-04", MainModule = "Arkadaşına Öner", SubScreen = "Arkadaşına Öner", Title = "Referans Kampanyası Şartlar ve Koşullar Bilgilendirmesi", AcceptanceCriteria = "Kampanya kuralları (örn: davet edilen kişinin ilk transferi yapması şartı) detaylı link ile sunulmalıdır.", ExpectedResult = "'Kampanya Koşulları' tıklandığında bilgilendirme sayfası eksiksiz açılmalıdır.", Status = "Bekliyor" },

            // 7. DOKÜMANLAR
            new() { Id = "TC-DOC-01", MainModule = "Dokümanlar", SubScreen = "Dokümanlar", Title = "Onay Bekleyen Dokümanların Görüntülenmesi ve Müşteri Onayı", AcceptanceCriteria = "Bankanın müşteriye ilettiği onay bekleyen sözleşme ve bildirim evrakları ayrı bir listede gösterilmeli; müşteri içeriği inceleyip dijital onay verebilmelidir.", ExpectedResult = "Doküman tıklandığında önizleme açılmalı, 'Okudum, Onaylıyorum' butonuna basıldığında doküman durumu 'Onaylandı' olarak güncellenmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-DOC-02", MainModule = "Dokümanlar", SubScreen = "Dokümanlar", Title = "Aylık Hesap Özeti (Kontoauszug) PDF İndirme", AcceptanceCriteria = "Kullanıcı seçtiği ay ve yıla ait hesap ekstresini PDF formatında cihazına indirebilmelidir.", ExpectedResult = "PDF dosyası bozulmadan açılmalı ve banka resmi bilgilerini içermelidir.", Status = "Bekliyor" },
            new() { Id = "TC-DOC-03", MainModule = "Dokümanlar", SubScreen = "Dokümanlar", Title = "Yıllık Vergi Belgesi (Jahressteuerbescheinigung) Listeleme", AcceptanceCriteria = "Önceki vergilendirme yıllarına ait resmi vergi dökümleri filtrelenip indirilebilmelidir.", ExpectedResult = "Geçerli mali yıl seçildiğinde sistem üretilmiş resmi onaylı belgeyi indirmeye sunmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-DOC-04", MainModule = "Dokümanlar", SubScreen = "Dokümanlar", Title = "Okunmamış Yeni Doküman Bildirim Rozeti (Badge)", AcceptanceCriteria = "Bankanın ilettiği yeni bir resmi bildirim veya ekstre varsa menü üzerinde kırmızı sayaç yanmalıdır.", ExpectedResult = "Belge görüntülendiğinde rozet sayısı otomatik olarak 1 azalmalıdır.", Status = "Bekliyor" },

            // 8. BİZE YAZIN
            new() { Id = "TC-SUP-01", MainModule = "Bize Yazın", SubScreen = "Bize Yazın", Title = "Destek Talebi Mesaj Gönderimi ve Karakter Sınırı", AcceptanceCriteria = "Kullanıcı konu başlığı seçerek en az 20, en fazla 500 karakterlik mesaj gönderebilmelidir.", ExpectedResult = "Başarılı gönderim sonrası 'Talebiniz alınmıştır, Takip No: #...' bildirim ekranı açılmalıdır.", Status = "Bekliyor" },
            new() { Id = "TC-SUP-02", MainModule = "Bize Yazın", SubScreen = "Bize Yazın", Title = "Destek Mesajına Ek Dosya / Ekran Görüntüsü Yükleme", AcceptanceCriteria = "Kullanıcı yaşadığı teknik soruna dair görsel veya PDF yükleyebilmeli (Maksimum 2 adet, 5 MB)", ExpectedResult = "Desteklenmeyen dosya türlerinde (örn: .exe) format uyarısı verilerek yükleme reddedilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SUP-03", MainModule = "Bize Yazın", SubScreen = "Bize Yazın", Title = "Geçmiş Destek Talepleri ve Statü Takibi", AcceptanceCriteria = "Daha önce açılan taleplerin durumu (İnceleniyor, Yanıtlandı, Kapandı) liste halinde görülebilmelidir.", ExpectedResult = "Müşteri temsilcisinin verdiği yanıtlar mesaj detayı tıklandığında okunabilmelidir.", Status = "Bekliyor" },
            new() { Id = "TC-SUP-04", MainModule = "Bize Yazın", SubScreen = "Bize Yazın", Title = "Müşteri İletişim Tercihi Seçimi (Telefon / E-posta)", AcceptanceCriteria = "Kullanıcı geri dönüşün hangi kanal üzerinden yapılmasını istediğini formda seçebilmelidir.", ExpectedResult = "Seçilen iletişim kanalı talep özetinde müşteri temsilcisi notlarına eklenmelidir.", Status = "Bekliyor" }
        };
    }

    private List<TestScenario> LoadData()
    {
        lock (FileLock)
        {
            if (!System.IO.File.Exists(DataFilePath))
            {
                var initial = GetInitialData();
                SaveData(initial);
                return initial;
            }
            var json = System.IO.File.ReadAllText(DataFilePath);
            return JsonSerializer.Deserialize<List<TestScenario>>(json) ?? new List<TestScenario>();
        }
    }

    private void SaveData(List<TestScenario> data)
    {
        lock (FileLock)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(DataFilePath, json);
        }
    }

    public IActionResult Index(string? module, string? screen, string? executedModule)
    {
        var allList = LoadData();

        var executedQuery = allList.Where(s => s.Status == "Geçti" || s.Status == "Kaldı");
        if (!string.IsNullOrEmpty(executedModule) && executedModule != "Tümü")
        {
            executedQuery = executedQuery.Where(s => s.MainModule == executedModule);
        }

        var model = new TrackerViewModel
        {
            SelectedModule = module,
            SelectedScreen = screen,
            ExecutedModuleFilter = string.IsNullOrEmpty(executedModule) ? "Tümü" : executedModule,
            TotalScenariosCount = allList.Count,
            PassedCount = allList.Count(s => s.Status == "Geçti"),
            FailedCount = allList.Count(s => s.Status == "Kaldı"),
            PendingCount = allList.Count(s => s.Status == "Bekliyor"),
            ExecutedScenarios = executedQuery.OrderByDescending(s => s.ExecutedAt ?? DateTime.MinValue).ToList()
        };

        if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(screen) || module == "Seçiniz" || screen == "Seçiniz")
        {
            model.HasFiltered = false;
            model.FilteredScenarios = new List<TestScenario>();
        }
        else
        {
            model.HasFiltered = true;
            model.FilteredScenarios = allList.Where(x => x.MainModule == module && x.SubScreen == screen).ToList();
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public IActionResult UpdateStatus(string id, string status, string testerNote, string returnModule, string returnScreen, string returnExecutedModule)
    {
        var list = LoadData();
        var target = list.FirstOrDefault(x => x.Id == id);
        if (target != null)
        {
            target.Status = status;
            target.TesterNote = testerNote ?? string.Empty;
            target.ExecutedAt = (status == "Bekliyor") ? null : DateTime.Now;
            SaveData(list);
        }
        return RedirectToAction("Index", new { module = returnModule, screen = returnScreen, executedModule = returnExecutedModule });
    }

    [HttpPost]
    public IActionResult CreateScenario(string mainModule, string subScreen, string title, string acceptanceCriteria, string expectedResult)
    {
        var list = LoadData();
        string prefix = mainModule.Contains("Hesap") ? "TC-ACC" :
                        mainModule.Contains("Transfer") ? "TC-TRF" :
                        mainModule.Contains("Döviz") ? "TC-FX" :
                        mainModule.Contains("Kart") ? "TC-CRD" :
                        mainModule.Contains("Ayar") ? "TC-SET" :
                        mainModule.Contains("Arkadaş") ? "TC-REF" :
                        mainModule.Contains("Doküman") ? "TC-DOC" : "TC-SUP";

        var newScenario = new TestScenario
        {
            Id = $"{prefix}-{new Random().Next(100, 999)}",
            MainModule = mainModule,
            SubScreen = subScreen,
            Title = title,
            AcceptanceCriteria = acceptanceCriteria,
            ExpectedResult = expectedResult,
            Status = "Bekliyor",
            TesterNote = string.Empty,
            ExecutedAt = null
        };

        list.Add(newScenario);
        SaveData(list);

        return RedirectToAction("Index", new { module = mainModule, screen = subScreen });
    }
}
