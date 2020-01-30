#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;
using World = DragonsSpine.GameWorld.World;

namespace DragonsSpine.GameSystems.Text
{
    public static class NameGenerator
    {
        public const int NAME_MIN_LENGTH = 4;
        public const int NAME_MAX_LENGTH = 14;

        public static string[] GuildTags = new string[]
        {
            "AAH","AAL","AAS","ABA","ABO","ABS","ABY","ACE","ACT","ADD","ADO","ADS","ADZ","AFF","AFT","AGA","AGE","AGO","AGS","AHA","AHI","AHS","AID","AIL","AIM","AIN","AIR","AIS","AIT","ALA","ALB","ALE","ALL","ALP","ALS","ALT","AMA","AMI","AMP","AMU","ANA","AND","ANE","ANI","ANT","ANY","APE","APO","APP","APT","ARB","ARC","ARE","ARF","ARK","ARM","ARS","ART","ASH","ASK","ASP","ASS","ATE","ATT","AUK","AVA","AVE","AVO","AWA","AWE","AWL","AWN","AXE","AYE","AYS","AZO","BAA","BAD","BAG","BAH","BAL","BAM","BAN","BAP","BAR","BAS","BAT","BAY","BED","BEE","BEG","BEL","BEN","BES","BET","BEY","BIB","BID","BIG","BIN","BIO","BIS","BIT","BIZ","BOA","BOB","BOD","BOG","BOO","BOP","BOS","BOT","BOW","BOX","BOY","BRA","BRO","BRR","BUB","BUD","BUG","BUM","BUN","BUR","BUS","BUT","BUY","BYE","BYS","CAB","CAD","CAM","CAN","CAP","CAR","CAT","CAW","CAY","CEE","CEL","CEP","CHI","CIG","CIS","COB","COD","COG","COL","CON","COO","COP","COR","COS","COT","COW","COX","COY","COZ","CRU","CRY","CUB","CUD","CUE","CUM","CUP","CUR","CUT","CWM","DAB","DAD","DAG","DAH","DAK","DAL","DAM","DAN","DAP","DAW","DAY","DEB","DEE","DEF","DEL","DEN","DEV","DEW","DEX","DEY","DIB","DID","DIE","DIF","DIG","DIM","DIN","DIP","DIS","DIT","DOC","DOE","DOG","DOL","DOM","DON","DOR","DOS","DOT","DOW","DRY","DUB","DUD","DUE","DUG","DUN","DUO","DUP","DYE","EAR","EAT","EAU","EBB","ECU","EDH","EDS","EEK","EEL","EFF","EFS","EFT","EGG","EGO","EKE","ELD","ELF","ELK","ELL","ELM","ELS","EME","EMS","EMU","END","ENG","ENS","EON","ERA","ERE","ERG","ERN","ERR","ERS","ESS","ETA","ETH","EVE","EWE","EYE","FAB","FAD","FAG","FAN","FAR","FAS","FAT","FAX","FAY","FED","FEE","FEH","FEM","FEN","FER","FES","FET","FEU","FEW","FEY","FEZ","FIB","FID","FIE","FIG","FIL","FIN","FIR","FIT","FIX","FIZ","FLU","FLY","FOB","FOE","FOG","FOH","FON","FOP","FOR","FOU","FOX","FOY","FRO","FRY","FUB","FUD","FUG","FUN","FUR","GAB","GAD","GAE","GAG","GAL","GAM","GAN","GAP","GAR","GAS","GAT","GAY","GED","GEE","GEL","GEM","GEN","GET","GEY","GHI","GIB","GID","GIE","GIG","GIN","GIP","GIT","GNU","GOA","GOB","GOD","GOO","GOR","GOS","GOT","GOX","GOY","GUL","GUM","GUN","GUT","GUV","GUY","GYM","GYP","HAD","HAE","HAG","HAH","HAJ","HAM","HAO","HAP","HAS","HAT","HAW","HAY","HEH","HEM","HEN","HEP","HER","HES","HET","HEW","HEX","HEY","HIC","HID","HIE","HIM","HIN","HIP","HIS","HIT","HMM","HOB","HOD","HOE","HOG","HON","HOP","HOT","HOW","HOY","HUB","HUE","HUG","HUH","HUM","HUN","HUP","HUT","HYP","ICE","ICH","ICK","ICY","IDS","IFF","IFS","IGG","ILK","ILL","IMP","INK","INN","INS","ION","IRE","IRK","ISM","ITS","IVY","JAB","JAG","JAM","JAR","JAW","JAY","JEE","JET","JEU","JIB","JIG","JIN","JOB","JOE","JOG","JOT","JOW","JOY","JUG","JUN","JUS","JUT","KAB","KAE","KAF","KAS","KAT","KAY","KEA","KEF","KEG","KEN","KEP","KEX","KEY","KHI","KID","KIF","KIN","KIP","KIR","KIS","KIT","KOA","KOB","KOI","KOP","KOR","KOS","KUE","KYE","LAB","LAC","LAD","LAG","LAM","LAP","LAR","LAS","LAT","LAV","LAW","LAX","LAY","LEA","LED","LEE","LEG","LEI","LEK","LET","LEU","LEV","LEX","LEY","LEZ","LIB","LID","LIE","LIN","LIP","LIS","LIT","LOB","LOG","LOO","LOP","LOT","LOW","LOX","LUG","LUM","LUV","LUX","LYE","MAC","MAD","MAE","MAG","MAN","MAP","MAR","MAS","MAT","MAW","MAX","MAY","MED","MEG","MEL","MEM","MEN","MET","MEW","MHO","MIB","MIC","MID","MIG","MIL","MIM","MIR","MIS","MIX","MOA","MOB","MOC","MOD","MOG","MOL","MOM","MON","MOO","MOP","MOR","MOS","MOT","MOW","MUD","MUG","MUM","MUN","MUS","MUT","MYC","NAB","NAE","NAG","NAH","NAM","NAN","NAP","NAW","NAY","NEB","NEE","NEG","NET","NEW","NIB","NIL","NIM","NIP","NIT","NIX","NOB","NOD","NOG","NOH","NOM","NOO","NOR","NOS","NOT","NOW","NTH","NUB","NUN","NUS","NUT","OAF","OAK","OAR","OAT","OBA","OBE","OBI","OCA","ODA","ODD","ODE","ODS","OES","OFF","OFT","OHM","OHO","OHS","OIL","OKA","OKE","OLD","OLE","OMS","ONE","ONO","ONS","OOH","OOT","OPE","OPS","OPT","ORA","ORB","ORC","ORE","ORS","ORT","OSE","OUD","OUR","OUT","OVA","OWE","OWL","OWN","OXO","OXY","PAC","PAD","PAH","PAL","PAM","PAN","PAP","PAR","PAS","PAT","PAW","PAX","PAY","PEA","PEC","PED","PEE","PEG","PEH","PEN","PEP","PER","PES","PET","PEW","PHI","PHT","PIA","PIC","PIE","PIG","PIN","PIP","PIS","PIT","PIU","PIX","PLY","POD","POH","POI","POL","POM","POP","POT","POW","POX","PRO","PRY","PSI","PST","PUB","PUD","PUG","PUL","PUN","PUP","PUR","PUS","PUT","PYA","PYE","PYX","QAT","QIS","QUA","RAD","RAG","RAH","RAI","RAJ","RAM","RAN","RAP","RAS","RAT","RAW","RAX","RAY","REB","REC","RED","REE","REF","REG","REI","REM","REP","RES","RET","REV","REX","RHO","RIA","RIB","RID","RIF","RIG","RIM","RIN","RIP","ROB","ROC","ROD","ROE","ROM","ROT","ROW","RUB","RUE","RUG","RUM","RUN","RUT","RYA","RYE","SAB","SAC","SAE","SAG","SAL","SAP","SAT","SAU","SAW","SAX","SAY","SEA","SEC","SEE","SEG","SEI","SEL","SEN","SER","SET","SEW","SHA","SHE","SHH","SHY","SIB","SIC","SIM","SIN","SIP","SIR","SIS","SIT","SIX","SKA","SKI","SKY","SLY","SOB","SOD","SOL","SOM","SON","SOP","SOS","SOT","SOU","SOW","SOX","SOY","SPA","SPY","SRI","STY","SUB","SUE","SUK","SUM","SUN","SUP","SUQ","SYN","TAB","TAD","TAE","TAG","TAJ","TAM","TAN","TAO","TAP","TAR","TAS","TAT","TAU","TAV","TAW","TAX","TEA","TED","TEE","TEG","TEL","TEN","TET","TEW","THE","THO","THY","TIC","TIE","TIL","TIN","TIP","TIS","TIT","TOD","TOE","TOG","TOM","TON","TOO","TOP","TOR","TOT","TOW","TOY","TRY","TSK","TUB","TUG","TUI","TUN","TUP","TUT","TUX","TWA","TWO","TYE","UDO","UGH","UKE","ULU","UMM","UMP","UNS","UPO","UPS","URB","URD","URN","URP","USE","UTA","UTE","UTS","VAC","VAN","VAR","VAS","VAT","VAU","VAV","VAW","VEE","VEG","VET","VEX","VIA","VID","VIE","VIG","VIM","VIS","VOE","VOW","VOX","VUG","VUM","WAB","WAD","WAE","WAG","WAN","WAP","WAR","WAS","WAT","WAW","WAX","WAY","WEB","WED","WEE","WEN","WET","WHA","WHO","WHY","WIG","WIN","WIS","WIT","WIZ","WOE","WOG","WOK","WON","WOO","WOP","WOS","WOT","WOW","WRY","WUD","WYE","WYN","XIS","YAG","YAH","YAK","YAM","YAP","YAR","YAW","YAY","YEA","YEH","YEN","YEP","YES","YET","YEW","YID","YIN","YIP","YOB","YOD","YOK","YOM","YON","YOU","YOW","YUK","YUM","YUP","ZAG","ZAP","ZAS","ZAX","ZED","ZEE","ZEK","ZEP","ZIG","ZIN","ZIP","ZIT","ZOA","ZOO","ZUZ","ZZZ"
        };

        public static string[] FemaleNameEndings = new string[]
        {
            "ia", "aya", "la", "ya"
        };

        #region Name Origins
        //private enum BaseNameOrigins
        //{
        //    African,
        //    Albanian,
        //    Arabic,
        //    Armenian,
        //    Basque,
        //    Breton,
        //    Bulgarian,
        //    Catalan,
        //    Chinese,
        //    Cornish,
        //    Croatian,
        //    Czech,
        //    Danish,
        //    Dutch,
        //    English,
        //    Esperanto,
        //    Estonian,
        //    Finnish,
        //    French,
        //    Frisian,
        //    Galician,
        //    Georgian,
        //    German,
        //    Greek,
        //    Hawaiian,
        //    Hungarian,
        //    Icelandic,
        //    Indian,
        //    Indonesian,
        //    Iranian,
        //    Irish,
        //    Italian,
        //    Japanese,
        //    Jewish,
        //    Khmer,
        //    Korean,
        //    Latvian,
        //    Limburgish,
        //    Lithuanian,
        //    Macedonian,
        //    Manx,
        //    Maori,
        //    NativeAmerican,
        //    Norwegian,
        //    Occitan,
        //    Polish,
        //    Portuguese,
        //    Romanian,
        //    Russian,
        //    Scottish,
        //    Serbian,
        //    Slovak,
        //    Slovene,
        //    Spanish,
        //    Swedish,
        //    Thai,
        //    Turkish,
        //    Ukrainian,
        //    Vietnamese,
        //    Welsh
        //}

        //private enum MythologicalNameOrigins
        //{
        //    GreekMyth,
        //    RomanMyth,
        //    CelticMyth,
        //    NorseMyth,
        //    Hinduism
        //}

        //private enum AncientNameOrigins
        //{
        //    ClassicalGreek,
        //    ClassicalRoman,
        //    AncientCeltic,
        //    AncientGermanic
        //}

        //private enum OtherNameOrigins
        //{
        //    Biblical,
        //    History,
        //    Literature,
        //    Theology,
        //    Fairy,
        //    Goth,
        //    Hillbilly,
        //    Hippy,
        //    Kreatyve,
        //    Rapper,
        //    Transformer,
        //    Witch,
        //    Wrestler
        //}

        //private enum FantasyNameOrigins
        //{
        //    Gluttakh,
        //    Monstrall,
        //    Romanto,
        //    Simitiq,
        //    Tsang,
        //    Xalaxxi
        //} 
        #endregion

        #region Common human male names.
        static string[] HUMAN_MALE_NAMES = new string[] {"Alf","Eunei","Deui","Emaoa","Olaozi","Juado","Euho","Iheia","Koemo","Uiquo","Iujeua",
                "Eurui","Iyeojo","Eulioi","Qioqu","Iefoeoo","Euneea","Eazua","Seihot","Lanekob","Juruew","Johokh","Tzcl","Skik","Bhym",
                "Juis","Xyoak","Bdiavub","Uivie","Eote","Ezeei","Auqie","Eupaia","Osioce","Agoido","Oameio","Iogoe","Zeirue","Ikuue",
                "Zoef","Haup","Jaych","Wutram","Asirerd","Etho","Truf","Areag","Ereit","Cia","Etho",
                "Abaet","Abarden","Aboloft","Acamen","Achard","Ackmard","Adeen","Aerden","Afflon","Aghon","Agnar","Ahalfar","Ahburn","Ahdun","Airen",
                "Airis","Aldaren","Aldren","Alkirk","Allso","Amerdan","Amitel","Anfar","Anumi","Anumil","Asden","Asdern","Asen","Aslan","Atar","Atgur",
                "Atlin","Auchfor","Auden","Ault","Ayrie","Aysen","Bacohl","Badeek","Baguk","Balati","Bapader","Barkydle","Basden","Bayde",
                "Bedic","Beeron","Bein","Beson","Betur","Bevurlde","Bewul","Biedgar","Bildon","Biston","Bithon","Boal","Bocldelr","Bolrock",
                "Brakdern","Bredere","Bredin","Bredock","Breen","Bristan","Buchmeid","Bue","Busma","Buthomar","Bydern","Caelholdt","Cainon","Calden",
                "Camchak","Camilde","Cardon","Casden","Cayold","Celbahr","Celorn","Celthric","Cemark","Cerdern","Cespar","Cether","Cevelt","Chamon",
                "Chesmarn","Chidak","Cibrock","Cipyar","Ciroc","Codern","Colthan","Cos","Cosdeer","Cuparun","Cusmirk","Cydare","Cylmar","Cythnar",
                "Cyton","Daburn","Daermod","Dak","Dakamon","Dakkone","Dalburn","Dalmarn","Dapvhir","Darkkon","Darko","Darmor","Darpick","Dasbeck",
                "Dask","Defearon","Derik","Derrin","Desil","Dessfar","Dinfar","Dismer","Doceon","Dochrohan","Dokoran","Dorn","Dosoman","Drakoe",
                "Drakone","Drandon","Drit","Dritz","Drophar","Dryden","Dryn","Duba","Dukran","Duran","Durmark","Dusaro","Dyfar","Dyten","Eard",
                "Efamar","Efar","Egmardern","Eiridan","Ekgamut","Elik","Elson","Elthin","Enbane","Endor","Enidin","Enoon","Enro","Erikarn","Erim",
                "Eritai","Escariet","Espardo","Etar","Etburn","Etdar","Ethen","Etmere","Etran","Eythil","Faoturk","Faowind","Fenrirr","Fetmar",
                "Feturn","Ficadon","Fickfylo","Fildon","Firedorn","Firiro","Floran","Folmard","Fraderk","Fronar","Fydar","Fyn","Gafolern","Galain",
                "Gauthus","Gemardt","Gemedern","Gemedes","Gerirr","Geth","Gib","Gibolock","Gibolt","Gith","Gom","Gosford","Gothar","Gothikar",
                "Gresforn","Grimie","Gryn","Gundir","Guthale","Gybol","Gybrush","Gyin","Halmar","Harrenhal","Hectar","Hecton","Heramon","Hermenze",
                "Hermuck","Hezak","Hildale","Hildar","Hileict","Hydale","Hyten","Iarmod","Idon","Ieli","Ieserk","Ikar","Ilgenar","Illilorn","Illium",
                "Ingel","Ipedorn","Isen","Isil","Ithric","Jackson","Jalil","Jamik","Janus","Jayco","Jaython","Jesco","Jespar","Jethil","Jex","Jib",
                "Jibar","Jin","Juktar","Julthor","Jun","Justal","Kafar","Kaldar","Kellan","Keran","Kesad","Kesmon","Kethren","Kib","Kibidon","Kiden",
                "Kilbas","Kilburn","Kildarien","Kimdar","Kinorn","Kip","Kirder","Kodof","Kolmorn","Kyrad","Lackus","Lacspor","Laderic","Lafornon",
                "Lahorn","Laracal","Ledale","Leit","Lephar","Lephidiles","Lerin","Lesphares","Letor","Lidorn","Lin","Liphanes","Loban","Lox",
                "Ludokrin","Luphildern","Lupin","Lurd","Macon","Madarlon","Mafar","Marderdeen","Mardin","Markard","Markdoon","Marklin","Mashasen",
                "Mathar","Medarin","Medin","Mellamo","Meowol","Merdon","Meridan","Merkesh","Mesah","Mesard","Mesophan","Mesoton","Mezo","Mickal",
                "Migorn","Milo","Miphates","Mitalrythin","Mitar","Modric","Modum","Mudon","Mufar","Mujarin","Mylo","Mythil","Nadeer","Nalfar",
                "Namorn","Naphates","Neowyld","Nidale","Nikpal","Nikrolin","Niktohal","Niro","Noford","Nothar","Nuthor","Nuwolf","Nydale","Nythil",
                "Obarho","Ocarin","Occelot","Occhi","Odaren","Odeir","Ohethlic","Okar","Omaniron","Omarn","Orin","Ospar","Othelen","Oxbaren",
                "Padan","Palid","Papur","Peitar","Pelphides","Pender","Pendus","Perder","Perol","Phairdon","Phemedes","Phexides","Picon","Pictal",
                "Picumar","Pildoor","Ponith","Poran","Poscidion","Prothalon","Puthor","Pyder","Qeisan","Qidan","Quiad","Quid","Quiss","Qupar",
                "Qysan","Radagmal","Randar","Raysdan","Rayth","Resboron","Reth","Rethik","Rhithik","Rhithin","Rhysling","Riandur","Rikar","Rismak",
                "Riss","Ritic","Rogeir","Rogist","Rogoth","Rophan","Rulrindale","Rydan","Ryfar","Ryfar","Ryodan","Rysdan","Rythen","Rythern","Sabal",
                "Sadareen","Safilix","Samon","Samot","Sasic","Scoth","Secor","Sed","Sedar","Senick","Senthyril","Serin","Sermak","Seryth","Sesmidat",
                "Setlo","Shardo","Shillen","Silco","Sildo","Silforrin","Silpal","Sithik","Sothale","Staph","Suktor","Suth","Sutlin","Syr","Syth",
                "Sythril","Talberon","Telpur","Temil","Temilfist","Teslanar","Tespar","Tessino","Tethran","Thiltran","Tholan","Ticharol","Tobale",
                "Tolkolie","Tolle","Tolsar","Toma","Tothale","Tousba","Tuk","Tuscanar","Tusdar","Tyden","Uerthe","Ugmar","Uhrd","Undin","Updar",
                "Uther","Vaccon","Vacone","Valynard","Vectomon","Veldahar","Vethelot","Vider","Vigoth","Vilan","Vildar","Vinald","Vinkolt","Virde",
                "Voltain","Volux","Voudim","Vythethi","Wakiern","Walkar","Wanar","Wekmar","Werymn","Weshin","Willican","Wilte","Wiltmar","Witfar",
                "Wuthmon","Wyder","Wyeth","Xander","Xavier","Xenil","Xex","Xithyl","Xuio","Yaeth","Yabaro","Yepal","Yesirn","Yssik","Yssith","Zak",
                "Zakarn","Zecane","Zeke","Zerin","Zessfar","Zidar","Zigmal","Zile","Zilocke","Zio","Zoru","Zotar","Zutar","Zyten"};
        #endregion

        #region Common human female names.
        static string[] HUMAN_FEMALE_NAMES = new string[] {"Wiceillan","Cren","Galaveth","Ociramwen","Jerayssi","Dwirari","Laroicia","Fen","Leran","Beliswen","Eowelavia",
                "Selirith","Haebeth","Ederawiel","Crilaveth","Aseillan","Mardoclya","Dirathien","Perassi","Olaesien","Veama","Kediraven","Cadaelian","Frinia",
                "Cadelani","Yiniel","Sevorerien","Olaresa","Kailacien","Gweamwen","Venia","Gausa","Qoen","Sedith","Cimma","Aberadia","Morewien","Lothymeth",
                "Ybirethiel","Larireni","Seviarith","Ferang","Graemma","Gleridia","Gweavia","Wode","Ybean","Kayrwen","Aceivia","Wicardovia",
                "Acele","Acholate","Ada","Adiannon","Adorra","Ahanna","Akara","Akassa","Akia","Amaerilde","Amara","Amarisa","Amarizi","Ana","Andonna",
                "Ani","Annalyn","Archane","Ariannona","Arina","Arryn","Asada","Awnia","Ayne","Basete","Bathelie","Bethe","Brana","Brianan","Bridonna",
                "Brynhilde","Calene","Calina","Celestine","Celoa","Cephenrene","Chani","Chivahle","Chrystyne","Corda","Cyelena","Dalavesta","Desini",
                "Dylena","Ebatryne","Ecematare","Efari","Enaldie","Enoka","Enoona","Errinaya","Fayne","Frederika","Frida","Gene","Gessane","Gronalyn",
                "Gvene","Gwethana","Halete","Helenia","Hildandi","Hyza","Idona","Ikini","Ilene","Illia","Iona","Jessika","Jezzine","Justalyne",
                "Kassina","Kilayox","Kilia","Kilyne","Kressara","Laela","Laenaya","Lelani","Lenala","Linovahle","Linyah","Lloyanda","Lolinda",
                "Lyna","Lynessa","Mehande","Melisande","Midiga","Mirayam","Mylene","Nachaloa","Naria","Narisa","Nelenna","Niraya","Nymira",
                "Ochala","Olivia","Onathe","Ondola","Orwyne","Parthinia","Pascheine","Pela","Perifilly","Pharysene","Philadona","Prisane","Prysala",
                "Pythe","Qara","Qiala","Quasee","Rhyanon","Rivatha","Ryiah","Sanala","Sathe","Senira","Sennetta","Sepherene","Serane","Sevestra",
                "Sidara","Sidathe","Sina","Sunete","Synestra","Sythini","Szene","Tabika","Tabithi","Tajule","Tamare","Teresse","Tolida","Tonica",
                "Treka","Tressa","Trinsa","Tryane","Tybressa","Tycane","Tysinni","Undaria","Uneste","Urda","Usara","Useli","Ussesa","Venessa",
                "Veseere","Voladea","Vysarane","Vythica","Wanera","Welisarne","Wellisa","Wesolyne","Wyeta","Yilvoxe","Ysane","Yve","Yviene",
                "Yvonnette","Yysara","Zana","Zathe","Zecele","Zenobia","Zephale","Zephere","Zerma","Zestia","Zilka","Zoura","Zrye","Zyneste","Zynoa"};
        #endregion

        /// <summary>
        /// Get a random name using Japanese syllabry. Currently called for eSpecies.Human NPCs spawning in Leng or Torii, and for NPCs with a race value of Leng.
        /// </summary>
        /// <param name="gender"></param>
        /// <returns></returns>
        public static string GetJapaneseName(Globals.eGender gender)
        {
            int length = Rules.RollD(1, 3) + 1;
            string name = "";
            for (int a = 0; a < length; a++)
            {
                if (a == 0)
                    name += TextManager.JAPANESE_SYLLABLES[Rules.Dice.Next(0, TextManager.JAPANESE_SYLLABLES.Length - 1)];
                else
                    name += TextManager.JAPANESE_SYLLABLES[Rules.Dice.Next(0, TextManager.JAPANESE_SYLLABLES.Length)];
            }

            // It is typical for a female's name to end with a ko, which means child. For example: Kayoko, depending on the kanji, may mean "child of the beautiful generation."
            if (gender == Globals.eGender.Female && Rules.RollD(1, 2) == 1)
                name += "ko";

            name = name.Substring(0, 1).ToUpper() + name.Substring(1);
            return name;
        }

        /// <summary>
        /// Get a random name for a Character (with a race, typically human or humanoid).
        /// </summary>
        /// <param name="ch">The Character object requiring a name.</param>
        /// <returns>A new, random name.</returns>
        public static string GetRandomName(Character ch)
        {
            if (ch.race == "Leng" || ch.species == Globals.eSpecies.Human && (World.GetFacetByIndex(0).GetLandByID(ch.LandID).Name == "Leng" || World.GetFacetByIndex(0).GetLandByID(ch.LandID).Name == "Torii"))
                return NameGenerator.GetJapaneseName(ch.gender);
            else if (ch.race == "" || (ch.species != Globals.eSpecies.Human || Rules.RollD(1, 100) >= 20))
                return NameGenerator.GenerateRandomName(ch);

            if (ch.gender == Globals.eGender.Male)
                return HUMAN_MALE_NAMES[Rules.Dice.Next(0, HUMAN_MALE_NAMES.Length - 1)];
            else
                return HUMAN_FEMALE_NAMES[Rules.Dice.Next(0, HUMAN_FEMALE_NAMES.Length - 1)];
        }

        /// <summary>
        /// Called upon NPC creation if the RandomName attribute is true.
        /// </summary>
        /// <param name="ch">The Character to receive the random name.</param>
        /// <returns>The randomly generated name.</returns>
        public static string GenerateRandomName(Character ch)
        {
            switch (ch.species)
            {
                case Globals.eSpecies.Kobold:
                    return GenerateTwoSyllableName(ch, true);
                default:
                    break;
            }

            string[] vowels = { "a", "a", "a", "e", "e", "e", "i", "i", "i", "o", "o", "o", "u", "u", "u" };
            string[] consts = { "b", "b", "b", "c", "d", "d", "d", "f", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "r", "r", "s", "s", "s", "t", "t", "t", "v", "w", "x", "y", "z" };
            string name = "";
            bool lco = false;

            switch (Rules.Dice.Next(1, 3))
            {
                case 1:
                    name = name + consts[Rules.Dice.Next(consts.Length)].ToUpper();
                    lco = true;
                    break;
                case 2:
                    name = name + vowels[Rules.Dice.Next(vowels.Length)].ToUpper();
                    lco = false;
                    break;
                default:
                    break;
            }

            for (int l = 0; l < Rules.Dice.Next(2, 5); l++)
            {
                if (lco)
                {
                    if (Rules.Dice.Next(1, 2) == 1)
                        name = name + vowels[Rules.Dice.Next(vowels.Length)];
                }
                switch (Rules.Dice.Next(1, 3))
                {
                    case 1:
                        name = name + consts[Rules.Dice.Next(consts.Length)];
                        lco = true;
                        break;
                    case 2:
                        name = name + vowels[Rules.Dice.Next(vowels.Length)];
                        lco = false;
                        break;
                    default:
                        break;
                }
            }

            // TODO: more variety for female name endings
            if (ch.species == Globals.eSpecies.Human && ch.gender == Globals.eGender.Female)
            {
                name += NameGenerator.FemaleNameEndings[Rules.Dice.Next(0, NameGenerator.FemaleNameEndings.Length - 1)];
            }

            return name;
        }

        /// <summary>
        /// Generate a two syllable name for a non-human NPC.
        /// </summary>
        /// <param name="ch">The Character to receive the name.</param>
        /// <param name="capitalizeSecondSyllable">Some species' capitalize the second syllable, namely Kobolds.</param>
        /// <returns>The generated two syllable name.</returns>
        public static string GenerateTwoSyllableName(Character ch, bool capitalizeSecondSyllable)
        {
            string[] vowels = { "a", "e", "i", "o", "u" };
            string[] consts = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "y", "z" };

            string name = consts[Rules.Dice.Next(consts.Length)].ToUpper();

            for (int l = 0; l < 3; l++)
            {
                switch (l)
                {
                    case 0:
                    case 2:
                        name = name + vowels[Rules.Dice.Next(vowels.Length)];
                        switch (ch.species)
                        {
                            case Globals.eSpecies.Orc:
                                if (Rules.RollD(1, 100) >= 50)
                                {
                                    if (l == 0)
                                        name = name + vowels[Rules.Dice.Next(vowels.Length)];
                                    else
                                    {
                                        if (Rules.RollD(1, 100) >= 35)
                                        {
                                            name = name + consts[Rules.Dice.Next(consts.Length)];
                                        }
                                        else name = name + vowels[Rules.Dice.Next(vowels.Length)];

                                        // TODO: add accent mark here for certain species'
                                    }
                                }
                                break;
                            case Globals.eSpecies.Kobold:
                            default:
                                break;
                        }
                        break;
                    default:
                        name = name + (capitalizeSecondSyllable ? consts[Rules.Dice.Next(consts.Length)].ToUpper() : consts[Rules.Dice.Next(consts.Length)]);
                        break;
                }
            }

            return name;
        }

        /// <summary>
        /// This method is deprecated after being contacted by the webmaster of the site used to pull random names.
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="surname"></param>
        /// <param name="matchRegex"></param>
        /// <returns></returns>
        public static string GetRandomNameFromWeb(Character chr, bool surname, bool matchRegex)
        {
            string url = "http://www.behindthename.com/random/random.php?number=1&";

            if(chr.gender == Globals.eGender.Female)
                url += "gender=f&";
            else url += "gender=m&";

            if (surname)
                url += "surname=&randomsurname=yes&all=no&usage_";
            else
                url += "surname=&all=no&usage_";

            if(chr.race == "Leng" || chr.MapID == GameWorld.Map.ID_TORII || chr.MapID == GameWorld.Map.ID_SHUKUMEI)
            {
                string[] asianOrigins = new string[] { "chi", "jap", "kor" };
                url += asianOrigins[Rules.Dice.Next(0, asianOrigins.Length - 1)];
            }
            else
            {
                // Greek Myth, Roman Myth, Celtic Myth, Norse Myth, Hinduism, German, French, English, Greek, Irish, Italian, Scottish, Spanish, History
                // Classical Greek, Classical Roman, Ancient Celtic, Ancient Germanic
                string[] origins = new string[]
                {"grem", "romm", "celm", "scam", "indm", "ger", "fre", "eng", "gre", "iri", "ita", "sco", "spa", "hist",
                    "grea", "roma", "cela", "teua"};

                url += origins[Rules.Dice.Next(0, origins.Length - 1)];
            }

            url += "=1";

            string name = "";

            System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z\s]*$");

            while (name.Length < NAME_MIN_LENGTH || name.Length > NAME_MAX_LENGTH)
            {
                try
                {
                    System.Net.WebClient webClient = new System.Net.WebClient();
                    byte[] raw = webClient.DownloadData(url);
                    string webData = System.Text.Encoding.UTF8.GetString(raw);

                    string blah = @"<a class=""plain"" href=""/name/";

                    name = webData.Substring(webData.IndexOf(blah) + blah.Length, 20);
                    name = name.Substring(0, name.IndexOf(">") - 1);
                    name = name[0].ToString().ToUpper() + name.Substring(1);
                    name = name.Replace(" ", "."); // replace spaces with period

                    if (matchRegex && !rg.IsMatch(name))
                    {
                        name = ""; // will loop again
                        continue;
                    }

                    // Remove numbers.
                    for (int i = 0; i < 10;i++ )
                        name = name.Replace(i.ToString(), "");
                }
                catch (System.Net.WebException ex)
                {
                    Utils.LogException(ex);
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }

            return name;
        }

        public static void SetRandomGuildTag(Character chr)
        {
            if (chr.Name.Length >= NAME_MAX_LENGTH - 4) return;

            string randomTag = GuildTags[Rules.Dice.Next(0, GuildTags.Length - 1)];

            // lower, middle upper, middle lower

            int numPeriods = Rules.RollD(1, 6);
            int casing = Rules.RollD(1, 4);

            switch(casing)
            {
                case 1:
                    randomTag = randomTag.ToLower();
                    break;
                case 2:
                    randomTag = randomTag.Substring(0, 1).ToUpper() + randomTag.Substring(1, 1).ToLower() + randomTag.Substring(2, 1).ToUpper();
                    break;
                default:
                    randomTag = randomTag.ToUpper();
                    // random 4 letter guild tag
                    if (Rules.RollD(1, 100) >= 90) randomTag = randomTag + GuildTags[Rules.Dice.Next(0, GuildTags.Length - 1)].Substring(0, 1).ToUpper();
                    break;
            }

            for (int a = 1; a <= numPeriods; a++)
                chr.Name = chr.Name + ".";

            while (chr.Name.Length + randomTag.Length > NAME_MAX_LENGTH)
                chr.Name = chr.Name.Remove(chr.Name.LastIndexOf("."), 1); // remove periods from the end before tag

            if (chr.Name.Contains("."))
            {
                chr.Name += randomTag;
            }
        }
    }
}
