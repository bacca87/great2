using System.ComponentModel.DataAnnotations.Schema;

namespace Great.Models.Database
{
    [Table("Factory")]
    public partial class Factory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long TransferType { get; set; }
        public bool IsForfait { get; set; }
        public bool NotifyAsNew { get; set; }
        public bool OverrideAddressOnFDL { get; set; }
        public string CountryCode { get; set; }

        [ForeignKey("TransferType")]
        public virtual TransferType TransferType1 { get; set; }
    }

    public enum ECountry : int
    {
        AF
        , AL
        , DZ
        , AD
        , AO
        , AG
        , SA
        , AR
        , AM
        , AU
        , AT
        , AZ
        , BS
        , BH
        , BD
        , BB
        , BE
        , BZ
        , BJ
        , BT
        , BY
        , BO
        , BA
        , BW
        , BR
        , BN
        , BG
        , BF
        , BI
        , KH
        , CM
        , CA
        , CV
        , TD
        , CL
        , CN
        , CY
        , VA
        , CO
        , KM
        , KP
        , KR
        , CI
        , CR
        , HR
        , CU
        , DK
        , DM
        , EC
        , EG
        , SV
        , AE
        , ER
        , EE
        , ET
        , FJ
        , PH
        , FI
        , FR
        , GA
        , GM
        , GE
        , DE
        , GH
        , JM
        , JP
        , DJ
        , JO
        , GR
        , GD
        , GT
        , GN
        , GW
        , GQ
        , GY
        , HT
        , HN
        , IN
        , ID
        , IR
        , IQ
        , IE
        , IS
        , MH
        , SB
        , IL
        , IT
        , KZ
        , KE
        , KG
        , KI
        , KW
        , LA
        , LS
        , LV
        , LB
        , LR
        , LY
        , LI
        , LT
        , LU
        , MK
        , MG
        , MW
        , MV
        , MY
        , ML
        , MT
        , MA
        , MR
        , MU
        , MX
        , FM
        , MD
        , MC
        , MN
        , ME
        , MZ
        , NA
        , NR
        , NP
        , NI
        , NE
        , NG
        , NO
        , NZ
        , OM
        , NL
        , PK
        , PW
        , PS
        , PA
        , PG
        , PY
        , PE
        , PL
        , PT
        , QA
        , GB
        , CZ
        , CF
        , CG
        , CD
        , DO
        , RO
        , RW
        , RU
        , KN
        , VC
        , WS
        , SM
        , LC
        , ST
        , SN
        , RS
        , SC
        , SL
        , SG
        , SY
        , SK
        , SI
        , SO
        , ES
        , LK
        , US
        , ZA
        , SD
        , SS
        , SR
        , SE
        , CH
        , SZ
        , TJ
        , TW
        , TZ
        , TH
        , TL
        , TG
        , TO
        , TT
        , TN
        , TR
        , TM
        , TV
        , UA
        , UG
        , HU
        , UY
        , UZ
        , VU
        , VE
        , VN
        , YE
        , ZM
        , ZW
    }
}