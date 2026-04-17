SELECT
    g.GL_ACCOUNT_ID,
    g.ACCOUNT_CODE,
    g.ACCOUNT_NAME,
    g.ACCOUNT_NAME_ARABIC,
    g.DESCRIPTION,
    g.GL_ACCOUNT_TYPE_ID,
    g.GL_ACCOUNT_CLASS_ID
FROM
    GL_ACCOUNT g
        INNER JOIN
    GL_ACCOUNT_ORGANIZATION org
    ON g.GL_ACCOUNT_ID = org.GL_ACCOUNT_ID
WHERE
        g.PARENT_GL_ACCOUNT_ID = '210000';

SELECT
    party_id,
    description
FROM
    Party
WHERE
        description LIKE '%عبدالله احمد عبدالحكم محمد عبدالصمد%'
   OR description LIKE '%الجارحي للخرسانة المسلحة%'
   OR description LIKE '%عوض عيد دومه صميده دومه%'
   OR description LIKE '%الجواد احمد مصطفى%'
   OR description LIKE '%محمد عبدالتواب%'
   OR description LIKE '%محمد عبدالفضيل%'
   OR description LIKE '%رمضان الموان%'
   OR description LIKE '%شدوان%'
   OR description LIKE '%هيثم%'
   OR description LIKE '%احمد علي السيد الدموكي%'
   OR description LIKE '%احمد الحصري%'
   OR description LIKE '%احمد لودر تبع صبحي%'
   OR description LIKE '%شركة بصمه كلين%'
   OR description LIKE '%احمد عبدالرحمن%'
   OR description LIKE '%شركة ويندو للتسويق والدعاية%'
   OR description LIKE '%محمد مصطفى الجمال%'
   OR description LIKE '%فاليو للمنتجات الخرسانيه%'
   OR description LIKE '%هاني مختار%'
   OR description LIKE '%الفجالة للادوات الصحية%'
   OR description LIKE '%الشركة الهندسية لتشكيل المعادن%'
   OR description LIKE '%م عماد سمير حنفي محمود%'
   OR description LIKE '%الشركة المصرية للاتصالات%'
   OR description LIKE '%احمد فاروق%'
   OR description LIKE '%ابناء الفيوم%'
   OR description LIKE '%عماد فتحي رضوان%'
   OR description LIKE '%ايه زد ستيل%'
   OR description LIKE '%حمدي صبري عطيه%'
   OR description LIKE '%عادل عبدالغني ابوحمود%'
   OR description LIKE '%مكتب التصميمات والاستشارات الهندسية (محسن مشهور)%'
   OR description LIKE '%محمود الجمال%'
   OR description LIKE '%ياسر محمد محمد سعودي%'
   OR description LIKE '%د/هيثم%'
   OR description LIKE '%خليفة خميس محمد%'
   OR description LIKE '%محمود رضا محمود%'
   OR description LIKE '%شركة المتحده%'
   OR description LIKE '%شركة تارجيت ميكس للخرسانه الجاهزة%'
   OR description LIKE '%الاستشاري احمد ماهر%'
   OR description LIKE '%م/محمد عطيه%'
   OR description LIKE '%احمد القطاوي - شركة نيو ميكس برازرز%'
   OR description LIKE '%سمارت جروب للاستشارات الهندسية%'
   OR description LIKE '%بلاك استور - احمد رجب%'
   OR description LIKE '%شركة مصر الكبري%'
   OR description LIKE '%يوسف عبدالفتاح سويلم%'
   OR description LIKE '%محمود صلاح%'
   OR description LIKE '%ياسر حامد عيسي%'
   OR description LIKE '%احمد جابر امين%'
   OR description LIKE '%محمد عويس كامل%'
   OR description LIKE '%عبدالرحمن وليد%'
   OR description LIKE '%حاتم علي محمد%'
   OR description LIKE '%مصطفى حبيب%'
   OR description LIKE '%ميتال تك%'
   OR description LIKE '%فتحي البنا%'
   OR description LIKE '%حمدي صبري%'
   OR description LIKE '%احمد ابوشوشة%'
   OR description LIKE '%عبد الحميد اسماعيل%'
   OR description LIKE '%محمد عبدالعاطي%'
   OR description LIKE '%مصطفى فاروق عبدالوهاب (مصطفى الناظر)%'
   OR description LIKE '%اسلام%'
   OR description LIKE '%ماجد المتولي%'
   OR description LIKE '%سعيد عبدالوهاب قمر%'
   OR description LIKE '%ياسر سلامة%'
   OR description LIKE '%ربيع البنا%'
   OR description LIKE '%اشرف مبروك%'
   OR description LIKE '%فرج عبدالوهاب%'
   OR description LIKE '%عادل امام صديق%'
   OR description LIKE '%سيد سليمان محمد%'
   OR description LIKE '%مصطفي السيد%'
   OR description LIKE '%اسلام جمال%'
   OR description LIKE '%مصطفى جمال%'
   OR description LIKE '%عمرو حسين%'
   OR description LIKE '%صبحي انترلوك%'
   OR description LIKE '%ياسر الدعبس%'
   OR description LIKE '%احمد عبدالحميد%'
   OR description LIKE '%ايمن كمال%'
   OR description LIKE '%ايمن النقاش%'
   OR description LIKE '%عاطف النجار%'
   OR description LIKE '%يوسف عبدالعظيم%'
   OR description LIKE '%ناصر عدلي ابو الحسن%'
   OR description LIKE '%شركة الكاميرات%'
   OR description LIKE '%محمد سيد رمضان%'
   OR description LIKE '%شركة الغول%'
   OR description LIKE '%رضا عيد متولي%'
   OR description LIKE '%وليد البدري%'
   OR description LIKE '%سعيد شعبان%'
   OR description LIKE '%اسلام ايمن يونس%'
   OR description LIKE '%محمد حمدي%'
   OR description LIKE '%شعبان%'
   OR description LIKE '%محمود صبحي%'
   OR description LIKE '%محمد عز العرب طه%'
   OR description LIKE '%هاني صلاح%'
   OR description LIKE '%الحاج بنداري%'
   OR description LIKE '%محمد عطيه%'
   OR description LIKE '%احمد علم الدين%'
   OR description LIKE '%محمد عبدالعليم%'
   OR description LIKE '%محمد عبده%'
   OR description LIKE '%محمد رضا عبدالفتاح%'
   OR description LIKE '%محمود عبدالعزيز%'
   OR description LIKE '%ابراهيم مختار%'
ORDER BY
    description;

INSERT INTO PARTY_GL_ACCOUNT (
    ORGANIZATION_PARTY_ID,
    PARTY_ID,
    ROLE_TYPE_ID,
    GL_ACCOUNT_TYPE_ID,
    GL_ACCOUNT_ID,
    LAST_UPDATED_STAMP,
    LAST_UPDATED_TX_STAMP,
    CREATED_STAMP,
    CREATED_TX_STAMP
) VALUES
      ('Company', '100', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210001', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '101', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210002', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '102', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210003', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '103', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210004', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '104', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210005', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '105', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210006', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '106', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210007', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '107', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210008', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '108', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210009', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '109', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210010', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '110', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210011', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '111', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210012', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '112', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210013', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '113', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210014', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '114', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210015', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '115', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210016', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '116', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210017', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '117', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210018', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '118', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210019', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '119', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210020', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '120', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210021', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '121', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210022', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '122', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210023', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '123', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210024', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '124', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210025', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '125', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210026', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '126', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210027', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '127', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210028', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '128', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210029', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '129', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210030', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '130', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210031', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '131', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210032', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '132', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210033', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '133', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210034', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '134', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210035', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '135', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210036', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '136', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210037', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '137', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210038', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '138', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210039', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '139', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210040', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '140', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210041', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '141', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210042', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '142', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210043', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '143', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210044', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '144', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210045', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '145', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210046', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '146', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210047', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '147', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210048', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '148', 'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210049', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '50',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210050', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '51',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210051', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '52',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210052', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '53',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210053', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '54',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210054', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '55',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210055', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '56',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210056', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '57',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210057', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '58',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210058', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '59',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210059', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '60',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210060', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '61',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210061', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '62',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210062', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '63',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210063', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '64',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210064', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '65',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210065', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '66',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210066', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '67',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210067', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '68',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210068', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '69',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210069', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '70',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210070', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '71',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210071', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '72',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210072', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '73',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210073', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '74',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210074', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '75',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210075', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '76',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210076', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '77',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210077', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '78',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210078', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '79',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210079', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '80',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210080', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '81',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210081', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '82',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210082', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '83',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210083', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '84',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210084', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '85',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210085', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '86',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210086', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '87',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210087', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '88',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210088', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '89',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210089', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '90',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210090', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '91',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210091', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '92',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210092', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '93',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210093', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '94',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210094', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '95',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210095', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '96',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210096', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '97',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210097', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00'),
      ('Company', '98',  'BILL_FROM_VENDOR', 'ACCOUNTS_PAYABLE', '210098', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00', '2026-01-30 17:30:00');


SELECT
    pga.PARTY_ID,
    p.description                  AS party_description,
    pga.GL_ACCOUNT_ID,
    g.ACCOUNT_NAME_ARABIC          AS gl_account_name_arabic,
    pga.ROLE_TYPE_ID,
    pga.GL_ACCOUNT_TYPE_ID
FROM
    PARTY_GL_ACCOUNT pga
        INNER JOIN Party p     ON pga.PARTY_ID = p.party_id
        INNER JOIN GL_ACCOUNT g ON pga.GL_ACCOUNT_ID = g.GL_ACCOUNT_ID
WHERE
        pga.ORGANIZATION_PARTY_ID = 'Company'
  AND pga.ROLE_TYPE_ID = 'BILL_FROM_VENDOR'
  AND pga.GL_ACCOUNT_TYPE_ID = 'ACCOUNTS_PAYABLE'
ORDER BY p.description;