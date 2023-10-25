using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mro;

namespace sfc {
   /**
	 * standard bussines values definitios, basically this are name parameters that are
	 * consider standard for communication between the client and bussines rules although
	 * is not mandatory the framework encourange it
	 */
   public struct key {
      public const string BATCHL = "batchl";
      public const string BATLINI = "batlini";
      public const string BATLFIN = "batlfin";

      public const string CBATINI = "cbatini";
      public const string CBATFIN = "cbatfin";
      public const string CPRDINI = "cprdini";
      public const string CPRDFIN = "cprdfin";
      public const string CLININI = "clinini";
      public const string CLINFIN = "clinfin";
      public const string CPRTINI = "cprtini";
      public const string CPRTFIN = "cprtfin";

      public const string CPLTINI = "cpalini";
      public const string CINSPIN = "cinspin";

      public const string BATCH = "batch";
      public const string BATINI = "batini";
      public const string BATFIN = "batfin";
      public const string PROD = "prod";
      public const string PRDINI = "prdini";
      public const string PRDFIN = "prdfin";
      public const string LINE = "line";
      public const string LININI = "linini";
      public const string LINFIN = "linfin";
      public const string PART = "part";
      public const string PRTINI = "prtini";
      public const string PRTFIN = "prtfin";

      public const string SKU = "sku";
      public const string QTY = "qty";
      public const string BARCODE = "barcode";
      public const string LOCATION = "location";
      public const string LOCINI = "locini";
      public const string LOCFIN = "locfin";
      public const string CYCLE = "cycle";

      public const string TYPE = "type";
      public const string OVEN = "oven";
      public const string NOINSP = "noinsp";
      public const string BLOCK = "block";
      public const string BLOCKSZ = "blocksize";
      public const string SAMPLE = "sample";
      public const string AQL = "aql";
      public const string DISPOSITION = "disposition";
      public const string OPERATOR = "operator";
      public const string ZONE = "zone";
      public const string DEFECT = "defect";
      public const string BULKPACK = "bulkpack";

      public const string CURPATH = "curpath";
      public const string DATE = "date";
      public const string STATUS = "status";

      public const string COMMENTS = "comments";
      public const string VARIATION = "variation";

      public const string LOT_RESINE = "lot_resine";
      public const string ISLEANLINE = "isleanline";

      public const string DISCOUNTFQ = "discountfq";

      public const string ISPULL = "ispull";

      public const string SAMPLECOUNT = "samplecount";

      public const string TOTAL = "total";

      public const string LOTNUMBER = "lotnumber";

      public const string LCELLA = "lcellA";
      public const string LCELLB = "lcellB";
      public const string LCELLC = "lcellC";
      public const string LCELLD = "lcellD";
      public const string LCELLE = "lcellE";
      public const string LCELLF = "lcellF";

      public const string IMAGE = "uimage";
   }

   // batch status
   public struct batchstatus {
      public const int WIPCST = 0;
      public const int RELCST = 1;
      public const int WIPCOT = 2;
      public const int RELCOT = 3;
      public const int RELQCT = 9;
      public const int RELEASE = 7;
      public const int HOLDED = 8;
      public const int RELCSTCOT = 15;
   }

   // standard status
   public struct status {
      public const string WIP = "WIP";
      public const string REL = "RELEASE";
   }

   // type quality
   public struct typeq {
      public const string FQ = "FQ";
      public const string RJ = "RJ";
      public const int DEFFQ = 0;
   }

   // standar locations
   public struct locs {
      public const string CST = "CST";    // casting
      public const string CSR = "CSR";    // casting reinspection
      public const string COT = "COT";    // coating
      public const string COR = "COR";    // coating reinspection
      public const string PKG = "PKG";    // packaging
      public const string PKR = "PKR";    // packaging reinspection
      public const string QCT = "QCT";    // quality control
      public const string FOC = "FOC";    // focovision
      public const string TOI = "TOI";    // transition
      public const string TOG = "TOG";    // transition generic
      public const string MOL = "MOL";    // moldloss
   }

   public struct consts {
      public static readonly BO.defect DEFFQ = new BO.defect("0");
      public static readonly BO.defect_type DEFTYPFG = new BO.defect_type(typeq.FQ);
      public static readonly BO.defect_type DEFTYPRJ = new BO.defect_type(typeq.RJ);

      public static readonly BO.defect_type DEFTYPQCCRT = new BO.defect_type("C");
      public static readonly BO.defect_type DEFTYPQCMAJ = new BO.defect_type("M");

      public static readonly BO.location LOCCST = new BO.location(locs.CST);
      public static readonly BO.location LOCCOT = new BO.location(locs.COT);
      public static readonly BO.location LOCQCT = new BO.location(locs.QCT);
      public static readonly BO.location LOCPKG = new BO.location(locs.PKG);
      public static readonly BO.location LOCMOL = new BO.location(locs.MOL);

      public static readonly BO.qc_inspection_level QCINSPCRT = new BO.qc_inspection_level(qcdeftype.CRT, "");
      public static readonly BO.qc_inspection_level QCINSPMLN = new BO.qc_inspection_level(qcdeftype.MLN, "");
      public static readonly BO.qc_inspection_level QCINSPMEM = new BO.qc_inspection_level(qcdeftype.MEM, "");

      //public static readonly BO.operador EMPTYOPER = new BO.operador("000000");
   }

   public struct qcdeftype {
      public const string CRT = "CRT";    // critics
      public const string MLN = "MLN";    // line majors
      public const string MEM = "MEM";    // packaging majors
   }
   public struct qcdisp {
      public const string REL = "REL";
      public const string REJ = "REJ";
   }
   public struct qcinspstatus {
      public const string REL = "REL";
      public const string WIP = "WIP";
   }
   public struct qcdefcat {
      public const string QC = "QC";
      public const string LIN = "LINEA";
      public const string PKG = "EMPAQUE";
   }

   public struct mse {
      public const string BATCH_BAD_FORMAT = "batch_bad_format";

      public const string INV_BATCH = "invalid_batch";
      public const string INV_PROD = "invalid_product";
      public const string INV_LINE = "invalid_line";
      public const string INV_PART = "invalid_part";

      public const string REG_NOT_EXIST = "reg_not_exist";
      public const string REG_ALREADY_EXIST = "reg_already_exist";

      public const string INSP_NOT_EXIST = "inspection_not_exist";
      public const string INSP_ALREADY_EXIST = "inspection_already_exist";
      public const string QCBLOCK_NOT_EXIST = "qcblock_not_exist";
      public const string DEFECT_NOT_EXIST = "defect_not_exist";
      public const string BASE_NOT_EXIST = "base_not_exist";

      public const string INSP_NOT_IN_WIP = "inspection_not_in_wip";

      public const string WRONG_FMT_BATCH = "wrong_format_batch";
      public const string WRONG_FMT_BATCH_INI = "wrong_format_batch_ini";
      public const string WRONG_FMT_BATCH_FIN = "wrong_format_batch_fin";
      public const string WRONG_FMT_PROD = "wrong_format_prod";
      public const string WRONG_FMT_PROD_INI = "wrong_format_prod_ini";
      public const string WRONG_FMT_PROD_FIN = "wrong_format_prod_fin";
      public const string WRONG_FMT_LINE = "wrong_format_line";
      public const string WRONG_BLOCK_INFORMATION = "wrong_block_information";

      public const string INC_DAT_DATE = "inc_dat_date";
      public const string INC_DAT_QTY = "inc_dat_qty";
      public const string INC_DAT_BATCH = "inc_dat_batch";
      public const string INC_DAT_BATCH_INI = "inc_dat_batch_ini";
      public const string INC_DAT_BATCH_FIN = "inc_dat_batch_fin";
      public const string INC_DAT_PROD = "inc_dat_prod";
      public const string INC_DAT_PROD_INI = "inc_dat_prod_ini";
      public const string INC_DAT_PROD_FIN = "inc_dat_prod_fin";
      public const string INC_DAT_LINE = "inc_dat_line";
      public const string INC_DAT_LINE_INI = "inc_dat_line_ini";
      public const string INC_DAT_LINE_FIN = "inc_dat_line_fin";
      public const string INC_DAT_PART = "inc_dat_part";
      public const string INC_DAT_PART_INI = "inc_dat_part_ini";
      public const string INC_DAT_PART_FIN = "inc_dat_part_fin";
      public const string INC_DAT_PART_SRC = "inc_dat_part_src";
      public const string INC_DAT_PART_DST = "inc_dat_part_des";
      public const string INC_DAT_USER = "inc_dat_user";
      public const string INC_DAT_COMMENTS = "inc_dat_comments";
      public const string INC_DAT_NOINSP = "inc_dat_noinsp";
      public const string INC_DAT_LINE_DST = "inc_dat_line_des";
      public const string INC_DAT_BASE = "inc_dat_base";
      public const string INC_DAT_ADD = "inc_dat_add";
      public const string INC_DAT_EYE = "inc_dat_eye";
      public const string INC_DAT_FB = "inc_dat_fb";
      public const string INC_DAT_MOLD = "inc_dat_mold";
      public const string INC_DAT_OPER = "inc_dat_oper";
      public const string INC_DAT_BLOCK = "inc_dat_block";
      public const string INC_DAT_DEFECT = "inc_dat_defect";
      public const string INC_DAT_SOURCE = "inc_dat_source";
      public const string INC_DAT_QCZONE = "inc_dat_qczone";
      public const string INC_DAT_CONSTRAINT = "inc_dat_constraint";
      public const string INC_DAT_BASE_TYPE = "inc_dat_base_type";
      public const string INC_DAT_DIAMMETER_INI = "inc_dat_diammeterini";
      public const string INC_DAT_DIAMMETER_FIN = "inc_dat_diammeterfin";
      public const string INC_DAT_BASE_INI = "inc_dat_start_base";
      public const string INC_DAT_BASE_FIN = "inc_dat_finish_base";
      public const string INC_DAT_WEIGHT = "inc_dat_weight";
      public const string INC_DAT_SKU = "inc_dat_sku";
      public const string INC_DAT_BARCODE = "inc_dat_barcode";
      public const string INC_DAT_BULKPACK = "inc_dat_bulkpack";
      public const string INC_DAT_FAM = "inc_dat_fam";
      public const string INC_DAT_PALLET = "inc_dat_pallete";
      public const string INC_DAT_FRONT = "inc_dat_front";
      public const string INC_DAT_BACK = "inc_dat_back";
      public const string INC_DAT_LOC = "inc_dat_loc";
      public const string INC_DAT_CYCLE = "inc_dat_cycle";
      public const string INC_DAT_TROLLEY = "inc_dat_trolley";
      public const string INC_DAT_DEP = "dat_inc_depto";

      public const string CAP_MUSTBE_WIP = "cap_mustbe_wip";
      public const string CAP_MUSTBE_WIPCST = "cap_mustbe_wipcst";
      public const string CAP_MUSTBE_WIPCOT = "cap_mustbe_wipcot";
      public const string CAP_SOURCE_NOT_EXISTS = "cap_source_not_exist";
      public const string CAP_DESTINY_NOT_EXISTS = "cap_destiny_not_exist";

      public const string WRONG_TARGET_LINE = "wrong_line_target";
      public const string WRONG_CAPTURE_SOURCE = "wrong_capture_in_source";
      public const string PROD_INFO_ALREADY_EXISTS = "production_info_already_exist";
      public const string NEW_SKU_CANNOT_BE_NULL = "new_resource_cannot_be_null";

      public const string BASE_4_FRONT_NOT_FOUND = "base_for_front_not_found";
      public const string BASE_4_BACK_NOT_FOUND = "base_for_back_not_found";
      public const string FRONT_NOT_MATCH_BACK = "front_not_match_back";

      public const string BAT_SRC_MUSTBE_WIPCST = "bat_src_mustbe_wipcst";
      public const string BAT_DES_MUSTBE_WIPCST = "bat_des_mustbe_wipcst";
      public const string CAP_SRC_MUSTBE_WIPCST = "cap_src_mustbe_wipcst";
      public const string CAP_DES_MUSTBE_WIPCST = "cap_des_mustbe_wipcst";

      public const string ALL_PRODUCTS_ARE_RELEASED = "all_products_are_released";
      public const string CANNOT_CAPTURE_ZERO = "cannot_capture_zero";
      public const string QTY_TOO_HIGH = "qty_too_high";
      public const string QTY_ONLY_ONE = "qty_only_one";

      public const string MUST_BE_LEAN_LINE = "must_be_lean_line";
      public const string MUST_NOT_BE_LEAN_LINE = "leanlines_not_this_process";

      public const string BATCH_NOT_EXIST = "batch_not_exist";
      public const string BATCH_SOURCE_NOT_EXIST = "batch_source_not_exist";
      public const string BATCH_DESTINY_NOT_EXIST = "batch_destiny_not_exist";
      public const string BATCH_IS_RELEASED = "batch_on_final_release";
      public const string BATCH_TAKEN_BY_COAT = "batch_taken_by_coating";
      public const string BATCH_NOT_STATUS = "batch_not_status";
      public const string BATCH_MUSTBE_WIP = "batch_mustbe_in_wip";
      public const string BATCH_MUSTBE_HOLD = "bat_mustbe_holded";
      public const string BATCH_MUSTBE_RELCOT = "bat_mustbe_relcot";
      public const string BATCH_MUSTBE_WIPCST = "bat_mustbe_wipcst";
      public const string BATCH_MUSTBE_WIPCOT = "bat_mustbe_wipcot";

      public const string BATCH_HAS_CAST_INFO = "batch_has_cast_info";
      public const string BATCH_HAS_COAT_INFO = "batch_has_coat_info";
      public const string BATCH_HAS_QC_INFO = "batch_has_qc_info";
      public const string BATCH_HAS_PKG_INFO = "batch_has_pkg_info";
      public const string BATCH_HAS_COAT_CAST_REL = "batch_has_coat_cast_rel";

      public const string BATCH_HAS_REINSP_COAT_INFO = "batch_has_reinsp_coat_info";

      public const string PROD_NOT_EXISTS = "product_not_exist";
      public const string LOC_NOT_EXIST = "loc_not_exist";
      public const string BAT_DTL_NOT_EXISTS = "bat_dtl_not_exist";

      public const string MOLDLOSS_NOT_EXISTS = "moldloss_not_exist";
      public const string MOLDLOSS_ALREADY_EXISTS = "moldloss_already_exist";
      public const string MOLDLOSS_NOT_IN_WIP = "moldloss_not_in_wip";
      public const string MOLDLOSS_ALREADY_UPLOADED = "moldloss_already_uploaded";
      public const string PLAN_NOT_EXISTS = "plan_not_exist";
      public const string PLAN_SOURCE_NOT_EXISTS = "plan_source_not_exists";
      public const string PLAN_TARGET_ALREADY_EXISTS = "plan_target_already_exists";

      public const string QTY_ZERO = "error_qty_zero";

      public const string SAME_BATCH = "same_batch";
      public const string SAME_LINE = "same_line";
      public const string DEST_LINE_HAS_PALLETES = "des_line_has_pallets";
      public const string SRC_LINE_NOT_HAS_PALLETES = "src_line_doesnt_has_palletes";

      public const string WRONG_DATE_INI = "wrong_date_ini";
      public const string WRONG_DATE_FIN = "wrong_date_fin";
      public const string WRONG_FORMAT_COLS_NEEDED = "wrong_format_columns_needed_are";
      public const string WRONG_NO_INSPECTION = "wrong_no_inspection";
      public const string WRONG_STATUS = "wrong_status";
      public const string WRONG_FB = "wrong_FB";
      public const string WRONG_EYE = "wrong_eye";
      public const string WRONG_DEFECT_SOURCE = "wrong_defect_source";
      public const string WRONG_DEFECT = "wrong_defect";

      public const string DEFECT_NOT_BELONG_TO_LOC = "defect_not_belong_to_loc";

      public const string QC_HIS_FOR_LINE_NOT_EXISTS = "qc_history_for_line_not_exist";
      public const string QCBLOCK_LAST_NOT_RELEASED = "last_block_insp_not_released";
      public const string QCBLOCK_LAST_MUSTBE_REL = "last_qc_insp_mustbe_released";
      public const string QCBLOCK_INSP_MUSTBE_WIP = "qc_block_insp_mustbe_wip";
      public const string QCBLOCK_SIZE_EMPTY = "block_size_is_empty";
      public const string QC_ONLY_FOR_INSP_2_HIGHER = "only_for_inspection_2_or_higher";
      public const string QCBLOCK_SIZE_GREATER_LIMIT = "block_size_greater_than_limit";
      public const string ONLY_FOR_INSPECTION_1 = "only_for_inspection_1";
      public const string CANNOT_CALC_INSP_LEVEL = "cannot_calc_new_inspection_level";

      public const string NO_PRODUCTS_FOUND = "no_products_found";
      public const string PROD_NOT_BELONG_GROUP = "product_not_belong_to_right_group";
      public const string DIFFERENT_AQLS_IN_TROLLEY = "different_aqls_in_trolley";
      public const string CANNOT_GET_AQL_TYPE = "cannot_get_aql_type";
      public const string AQL_RANGE_NOT_REGISTERED = "aql_range_not_registered";
      public const string TROLLEY_WRONG_CAPTURE = "trolley_wrong_capture";
      public const string QC_TEST_NOT_COMPLETED = "qc_test_no_completed";
      public const string NOT_ENOUGH_SAMPLE_COLLECTED = "not_enough_sample_collected";
      public const string IF_HOLDED_NEED_REASON = "if_holded_need_reason";
      public const string REJECT_REASON_NOT_EXIST = "reject_reason_not_exist";

      public const string SKU_WRONG_FORMAT = "sku_wrong_format";
      public const string SKU_NOT_PLANNED = "sku_not_planned";
      public const string SKU_NOT_PRODUCED = "sku_not_produced";
      public const string SKU_ALREADY_EXIST = "sku_already_exist";
      public const string SKU_NOT_FOUND = "sku_not_found";
      public const string SKU_NOT_EXIST_ON_BLOCK = "sku_not_exist_on_trolley";

      public const string WRONG_LINE_SOURCE = "wrong_line_source";
      public const string WRONG_LINE_TARGET = "wrong_line_target";

      public const string SKU_NOT_PLANNED_NOT_VALIDATION = "sku_not_planned_not_validation";
      public const string PLAN_PALLETES_SOURCE_NOT_EXIST = "plan_pallete_source_not_exist";
      public const string PLAN_PALLETES_ALREADY_EXISTS = "plan_palletes_already_exist";
      public const string FRONT_MOLD_NOT_CONFIGURED = "front_mold_not_configured";
      public const string FRONT_MOLD_PLANNED_IN_OTHER_LINE = "front_mold_planned_line_pall_sku";
      public const string BACK_MOLD_PLANNED_IN_OTHER_LINE = "back_mold_planned_line_pall_sku";
      public const string BACK_MOLD_NOT_IN_PRODUCTION = "back_mold_not_in_production";

      public const string INVALID_QTY = "invalid_quantity";
      public const string MOLD_NOT_PLANNED = "mold_not_planned";
      public const string MOLD_ALREADY_IN_SAP = "mold_already_in_sap";
   }
   public struct logacts {
      public const string AUTHORIZE_BOX = "authorize_box";
      public const string UNAUTHORIZE_BOX = "unautorize_box";
      public const string UNUPLOAD_BOX = "unupload_box";
      public const string CREATE_BATCH = "create_batch";
   }
}
