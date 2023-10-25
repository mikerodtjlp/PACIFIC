using System;

namespace sfc.BO
{
   public class mold_lent_match
    {     
       public mold_lent_match()
       {
           sku = new resource();
           molde = new mold("");
           batch = new batch();
           batch.lotno = new lot("");
           pallet = new pallet("");  
         

       }

        public mold_lent_match(String batch_set, String molde_set, String module_set, String resource_set, String _1_set,
                               String _2_set, String _3_set, String _4_set, String d_set, String e_set, String f_set, String g_set)
         {                                   
            
             batch = new batch(batch_set, "", module_set, "");             
             molde = new mold(molde_set);
             modulo = module_set;            
             sku = new resource(resource_set);
             _1 = _1_set;
             _2 = _2_set;
             _3 = _3_set;
             _4 = _4_set;
             d = d_set;
             e = e_set;
             f = f_set;
             g = g_set;                
         }

        public mold_lent_match(String batch_set, String molde_set, String module_set, String resource_set, String valor, int pallet, int campo_set)
        {
            
            batch = new batch(batch_set, "", module_set, "");
            molde = new mold(molde_set);
            modulo = module_set;            
            sku = new resource(resource_set);
            valor_introducido = valor;
            pal = pallet;
            campo = campo_set;
           
        }

       public resource sku { get; set; }
       public mold molde { get; set; }

       public pallet pallet{ get; set; }       
       public batch batch { get; set; }

       public String modulo { get; set; }
       public int moldIdx { get; set; }
       public DateTime moldDate { get; set; }

       public bool revision { get; set; }
       public int revision_count { get; set; }
       /// <summary>
       /// este obtiene el objeto de pantalla 
       /// </summary>
       public String barcode_number { get; set; }
       public String validacion { get; set; }
       public String valor_introducido { get; set; }
       public int _img { get; set; }
       public int campo { get; set; }
       public String campo1 { get; set; }
       public String campo2 { get; set; }
       public String campo3 { get; set; }
       public String campo4 { get; set; }
       public String _1 { get; set; }
       public String _2 { get; set; }
       public String _3 { get; set; }
       public String _4 { get; set; }
       public String d { get; set; }
       public String e { get; set; }
       public String f { get; set; }
       public String g { get; set; }
       public int pal { get; set; }
       public String resultado { get; set; }
       public String showmsg { get; set; }
       



    }



}
