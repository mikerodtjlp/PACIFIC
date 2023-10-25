using System;
using System.Collections.Generic;
using System.Text;
using mro.BO;
using mro.DAL;
using mro;

namespace mro.BL
{
    public class Transactor_BL
    {
        public Transactor_BL(CParameters conns)
        {
            conns.get(defs.ZDFAULT, ref dbcode);
        }
        public readonly string dbcode = string.Empty;

        #region Métodos para la generación de html
        string
                sEnter          = "\r\n",
                sHtml           = string.Empty,
                sTitulo         = string.Empty,
                sBody           = string.Empty,
                sControles      = string.Empty,
                sForma1         = string.Empty,
                sForma2         = string.Empty,
                sScript         = string.Empty,
                sColumnas       = string.Empty,
                sAncho          = string.Empty,
                sInputs         = string.Empty,
                sEdiFields      = string.Empty,
                sAddFields      = string.Empty,
                sKey            = string.Empty,
                sKeyDesc        = string.Empty,
                sFunciones      = string.Empty;

        int iCol = 0, iAdd = 0, iEdi = 0, iColumnas = 0;

        public string generaHtml(string sTabla,
                                 string sTransaccion,
                                 List<metaDatosCatalogo> result)
        {
            sHtml           = string.Empty;
            sTitulo         = string.Empty;
            sBody           = string.Empty;
            sControles      = string.Empty;
            sForma1         = string.Empty;
            sForma2         = string.Empty;
            sScript         = string.Empty;
            sColumnas       = string.Empty;
            sAncho          = string.Empty;
            sInputs         = string.Empty;
            sEdiFields      = string.Empty;
            sAddFields      = string.Empty;
            sKey            = string.Empty;
            sKeyDesc        = string.Empty;
            sFunciones      = string.Empty;
            int iMaxSize = 0;
            int iUltimaLlave = 0;

            for (int i = 0 ; i < result.Count ; i++)
                if (result[i].constraint_Type.Equals("PRIMARY KEY")) {
                    iUltimaLlave = i;
                    if (result[i].column_name.Length > iMaxSize)
                        iMaxSize = result[i].column_name.Length;
                }
            for (int i = 0 ; i < result.Count ; i++) {
                var datos = result[i];
                if (i > 0 && datos.column_name.Equals(result[i - 1].column_name))
                    continue;
                if (datos.constraint_Type.Equals("PRIMARY KEY") && i < iUltimaLlave) {
                    sControles +=
                        "      " +
                        "<label size=\""           + iMaxSize  + "\" " +
                                "value=\"[!dbtxt!:" + datos.column_name + "]\">" +
                        "</label>\"" + sEnter +
                        "      " +
                        "<input id=\"$HK_"          + datos.column_name + "$\" " +  //Header key
                                "maxlength=\""      + datos.max_length  + "\" " +
                                "size=\""           + datos.max_length  + "\">" +
                        "</input>" + sEnter +
                        "      <br/>" + sEnter;
                }
                construyeGrid(datos,datos.constraint_Type.Equals("PRIMARY KEY"));
            }
            sEdiFields += "              " +
                          "\"totedis\":\"" + iEdi + "\"" + sEnter;
            sAddFields += "              " +
                          "\"totadds\":\"" + iAdd + "\"" + sEnter;

            sForma1 =
                "    <form id=\"" + sTransaccion + "\">" + sEnter +
                "      <include>tbars\\xhtml\\tbarbeg.xml[|title|:[!dbmod!:" + sTransaccion + "]]</include>" + sEnter +
                "      <include>tbars\\xhtml\\tbarend.xml</include>" + sEnter +
                sControles +
                "      <br/>" + sEnter +
                "      <mrotable id=\"list0\" style=\"width:700px;\">" + sEnter +
                "        <include>" + sEnter +
                "          [genlist:" + sEnter +
                "            [id:0]" + sEnter +
                "            [nrows:16]" + sEnter +
                "            [ncols:" + iColumnas + "]" + sEnter +
                sColumnas +
                "          ]" + sEnter +
                "        </include>" + sEnter +
                "      </mrotable>" + sEnter +
                "      <include>tctrls\\xhtml\\ctrl_totalbarbtns.xml[|listid|:0]</include>" + sEnter +
                "    </form>" + sEnter;
            sForma2 =
                "    <form id=\"formcap0\" class=\"formcap\">" + sEnter +
                sInputs +
                "    </form>" + sEnter;
            sScript =
                "  <script type=\"text/javascript\">" + sEnter +
                "    module=\"TRANSACTOR\";" + sEnter +
                "    codebehind=\"Transactor.aspx\";" + sEnter +
                "    lprms='\"onedit0\"   :{\"useonlist\":\"1\",\"override\":\"actualizaCatalogo\"}," + sEnter +
                "           \"oninsert0\" :{\"useonlist\":\"1\",\"override\":\"insertaEnCatalogo\"}," + sEnter +
                "           \"ondelete0\" :{\"useonlist\":\"1\",\"override\":\"eliminaDeCatalogo\"}," + sEnter +
                "           \"onenter\"   :{\"override\":\"seleccionaDeCatalogo\"}," + sEnter +
                "           \"edifields0\":{" + sEnter +
                sEdiFields +
                "           }," + sEnter +
                "           \"addfields0\":{" + sEnter +
                sAddFields +
                "           }," + sEnter +
                "           \"imgfields0\":{\"img0\":\"7020\",\"img1\":\"7017\",\"img2\":\"7018\",\"totimgs\":\"3\"}," + sEnter +
                "          ';" + sEnter +
                "  <" + "/script" + ">" + sEnter;
            sTitulo = "  <" + "title" + ">" + sTransaccion + "<" + "/title" + ">" + sEnter;
            sBody =
                "  <" + "body" + ">" + sEnter +
                sForma1 + sForma2 +
                "  <" + "/body" + ">" + sEnter +
                sScript;
            sHtml =
                "<" + "html" + ">" + sEnter +
                sTitulo +
                sBody +
                "<" + "/html" + ">";

            return sHtml;
        }

        private void construyeGrid(metaDatosCatalogo datos,bool bEsLlave)
        {
            int iTopeLength = Convert.ToInt32(datos.max_length);
            if (datos.column_name.Length >=5 &&
                datos.column_name.Substring(datos.column_name.Length - 5,5).Equals("_date"))
                iTopeLength = 10;
            else if (iTopeLength > 20)
                iTopeLength = 20;
            string sNombreColumna = (bEsLlave ? "PK_":"") + datos.column_name;
            string sEspacios20 = "                    ";
            string sEspacios;
            if (datos.column_name.Length < 16)
                sEspacios = sEspacios20.Substring(0,16 - datos.column_name.Length);
            else
                sEspacios = string.Empty;
            sColumnas +=
                "              " +
                "[col" + iCol + ":" +
                "[name:[!dbtxt!:"   + datos.column_name + "]" + sEspacios + "]" +
                "[width:"           + iTopeLength  + "]]" + sEnter;
            if (sNombreColumna.Length < 16)
                sEspacios = sEspacios20.Substring(0,16 - sNombreColumna.Length);
            else
                sEspacios = string.Empty;
            sInputs   +=
                "      " +
                "<input id=\"$" + sNombreColumna + "$\"" + sEspacios + " " +
                "maxlength=\""  + datos.max_length  + "\" " + (datos.max_length.Length == 1 ? " ": "") +
                "size=\""   + iTopeLength  + "\"" + (datos.max_length.Length == 1 ? " ": "") + ">" +
                "</input>"  +
                sEnter;
            if (!columnaEspecial(datos.column_name)) {
                if (!bEsLlave) {
                    sEdiFields += "              \"edi" + iEdi + "\"   :\"$" + sNombreColumna + "$\"," + sEnter;
                    iEdi++;
                }
                sAddFields += "              \"add" + iAdd + "\"   :\"$" + sNombreColumna + "$\"," + sEnter;
                iAdd++;
            }
            iCol++;
            iColumnas++;
        }

        private bool columnaEspecial(string sColumna)
        {
            string[] sLista = {"created_by","creation_date","update_by","updated_date"};
            for (int i = 0 ; i < sLista.Length ; i++)
                if (sColumna.Equals(sLista[i]))
                    return true;
            return false;
        }
        
        /// <summary>
        /// Obtiene los metadatos de una tabla para la creación automática de la transacción en Html
        /// </summary>
        /// <param name="file">Base de datos, Tabla y Tipo (Key o no Key)</param>
        /// <returns></returns>
        public string generaTransaccion(string tabla, 
                                        string trans,
                                        string descripcionES,
                                        string descripcionEN,
                                        string descripcionPO,
                                        string descripcionGE,
                                        string descripcionCH,
                                        string usuario)
        {
            string html = string.Empty;
            err.require(string.IsNullOrEmpty(tabla), "inc_dat_table");
            err.require(string.IsNullOrEmpty(trans), "inc_dat_trans");

            using (Transactor_DAL pdal = Transactor_DAL.instance(dbcode))
            {
                List<metaDatosCatalogo> result = pdal.getMetadatosTabla(tabla);
                html = generaHtml(tabla,trans,result);
                foreach (var dato in result)
                    pdal.setDescripcionColumna(dato.column_name);
                if (descripcionEN == null || descripcionEN.Length == 0) descripcionEN = descripcionES;
                if (descripcionPO == null || descripcionPO.Length == 0) descripcionPO = descripcionES;
                if (descripcionGE == null || descripcionGE.Length == 0) descripcionGE = descripcionES;
                if (descripcionCH == null || descripcionCH.Length == 0) descripcionCH = descripcionES;
                pdal.setTransaccionHtml(trans, html,
                                        descripcionES, descripcionEN, descripcionPO,
                                        descripcionGE,descripcionCH);
                return "";
            }
        }

        /// <summary>
        /// Construye la consulta para obtener datos del catálogo correspondiente
        /// </summary>
        /// <param name="file">Vector de datos con el formato [$variable$:valor]</param>
        /// <returns></returns>
        public List<metaDatosTransactor> seleccionaDeCatalogo(string vector)
        {
            string
                sPedazo         = string.Empty,
                sVariable       = string.Empty,
                sValor          = string.Empty,
                sSql            = string.Empty,
                sListaVariables = string.Empty,
                sListaValores   = string.Empty,
                sCondiciones    = string.Empty,
                sTabla          = string.Empty,
                sBaseDatos      = string.Empty;
    
            while (vector.Length > 0) {
                sPedazo         = vector.Substring(0,vector.IndexOf(']') + 1);
                if (sPedazo.Substring(1,1).Equals("$")) {
                    sVariable       = sPedazo.Substring(2,sPedazo.IndexOf(':') - 3);
                    sValor          = sPedazo.Substring(sPedazo.IndexOf(':') + 1,
                                                        sPedazo.Length - sPedazo.IndexOf(':') - 2);
                    if (sVariable.Equals("_tabla"))
                        sTabla      = sValor;
                    else if (sVariable.Equals("_baseDatos"))
                        sBaseDatos  = sValor;
                    else if (sVariable.Substring(0,3).Equals("HK_")) {
                        if (sValor.Length > 0)
                            sCondiciones += (sCondiciones.Length > 0 ? " AND ":"") +
                                sVariable.Substring(3,sVariable.Length - 3) + "='" + sValor + "'";
                    }
                    else if (sVariable.Substring(0,3).Equals("PK_")) {
                        sVariable = "[" + sVariable.Substring(3,sVariable.Length - 3) + "]";
                        sListaVariables += (sListaVariables.Length > 0 ? "," : "") + sVariable;
                    }
                    else
                        sListaVariables += (sListaVariables.Length > 0 ? "," : "") + "[" + sVariable + "]";
                }
                vector = vector.Substring(vector.IndexOf(']') + 1,vector.Length - vector.IndexOf(']') - 1);
            }
            sSql =  "SELECT " + sListaVariables + " " +
                    "FROM "   + sBaseDatos + ".dbo." + sTabla + " ";
            if (sCondiciones.Length > 0)
                 sSql += "WHERE "  + sCondiciones;
            using (Transactor_DAL pdal = Transactor_DAL.instance(dbcode))
                return pdal.ejecutaSelectGenerado(sSql);
        }

        /// <summary>
        /// Construye la consulta para insertar en el catálogo correspondiente
        /// </summary>
        /// <param name="file">Vector de datos con el formato [$variable$:valor]</param>
        /// <returns></returns>
        public int insertaEnCatalogo(string vector,string usuario)
        {
            string
                sPedazo         = string.Empty,
                sVariable       = string.Empty,
                sValor          = string.Empty,
                sSql            = string.Empty,
                sListaVariables = string.Empty,
                sListaValores   = string.Empty,
                sTabla          = string.Empty,
                sBaseDatos      = string.Empty;
    
            while (vector.Length > 0) {
                sPedazo         = vector.Substring(0,vector.IndexOf(']') + 1);
                if (sPedazo.Substring(1,1).Equals("$")) {
                    sVariable       = sPedazo.Substring(2,sPedazo.IndexOf(':') - 3);
                    sValor          = sPedazo.Substring(sPedazo.IndexOf(':') + 1,
                                                        sPedazo.Length - sPedazo.IndexOf(':') - 2);
                    if (sVariable.Equals("_tabla"))
                        sTabla      = sValor;
                    else if (sVariable.Equals("_baseDatos"))
                        sBaseDatos  = sValor;
                    else if (sVariable.Equals("created_by")) {
                        sListaVariables += (sListaVariables.Length > 0 ? "," : "") + sVariable;
                        sListaValores   += (sListaValores.Length   > 0 ? "," : "") + "'" + usuario + "'";
                    }
                    else if (sVariable.Equals("creation_date")) {
                        sListaVariables += (sListaVariables.Length > 0 ? "," : "") + sVariable;
                        sListaValores   += (sListaValores.Length   > 0 ? "," : "") + "getdate()";
                    }
                    else if (!sVariable.Substring(0,3).Equals("HK_")) {
                        if (sValor.Length == 0)
                            sValor  = "null";
                        else
                            sValor  = "'" + sValor + "'";
                        if (sVariable.Substring(0,3).Equals("PK_"))
                            sVariable = sVariable.Substring(3,sVariable.Length - 3);
                        sListaVariables += (sListaVariables.Length > 0 ? "," : "") + sVariable;
                        sListaValores   += (sListaValores.Length   > 0 ? "," : "") + sValor;
                    }
                }
                vector = vector.Substring(vector.IndexOf(']') + 1,vector.Length - vector.IndexOf(']') - 1);
            }
            sSql = "INSERT INTO " + sBaseDatos + ".dbo." + sTabla + " (" + sListaVariables + ") VALUES (" + sListaValores + ")";
            using (Transactor_DAL pdal = Transactor_DAL.instance(dbcode))
                return pdal.ejecutaConsultaGenerada(sSql);
        }

        /// <summary>
        /// Construye la consulta para insertar en el catálogo correspondiente
        /// </summary>
        /// <param name="file">Vector de datos con el formato [$variable$:valor]</param>
        /// <returns></returns>
        public int actualizaCatalogo(string vector,string usuario)
        {
            string
                sPedazo         = string.Empty,
                sVariable       = string.Empty,
                sValor          = string.Empty,
                sSql            = string.Empty,
                sActualizaciones= string.Empty,
                sCondiciones    = string.Empty,
                sTabla          = string.Empty,
                sBaseDatos      = string.Empty;
    
            while (vector.Length > 0) {
                sPedazo         = vector.Substring(0,vector.IndexOf(']') + 1);
                if (sPedazo.Substring(1,1).Equals("$")) {
                    sVariable       = sPedazo.Substring(2,sPedazo.IndexOf(':') - 3);
                    sValor          = sPedazo.Substring(sPedazo.IndexOf(':') + 1,
                                                        sPedazo.Length - sPedazo.IndexOf(':') - 2);
                    if (sVariable.Equals("_tabla"))
                        sTabla      = sValor;
                    else if (sVariable.Equals("_baseDatos"))
                        sBaseDatos  = sValor;
                    else if (sVariable.Equals("update_by"))
                        sActualizaciones += (sActualizaciones.Length > 0 ? "," : "") +
                                            sVariable + "='"+ usuario + "'";
                    else if(sVariable.Equals("updated_date"))
                        sActualizaciones += (sActualizaciones.Length > 0 ? "," : "") +
                                            sVariable + "=getdate()";
                    else if (sVariable.Substring(0,3).Equals("PK_"))
                        sCondiciones += (sCondiciones.Length > 0 ? " AND " : "") +
                                        sVariable.Substring(3,sVariable.Length - 3) + "='" + sValor + "'";
                    else if (!sVariable.Substring(0,3).Equals("HK_")) {
                        if (sValor.Length == 0)
                            sValor  = "null";
                        else
                            sValor  = "'" + sValor + "'";
                        sActualizaciones += (sActualizaciones.Length > 0 ? "," : "") +
                                            "[" + sVariable + "]=" + sValor;
                    }
                }
                vector = vector.Substring(vector.IndexOf(']') + 1,vector.Length - vector.IndexOf(']') - 1);
            }
            sSql = "UPDATE " + sBaseDatos + ".dbo." + sTabla +
                    "  SET " + sActualizaciones +
                    " WHERE " + sCondiciones;
            using (Transactor_DAL pdal = Transactor_DAL.instance(dbcode))
                return pdal.ejecutaConsultaGenerada(sSql);
        }

        /// <summary>
        /// Construye la consulta para insertar en el catálogo correspondiente
        /// </summary>
        /// <param name="file">Vector de datos con el formato [$variable$:valor]</param>
        /// <returns></returns>
        public int eliminaDeCatalogo(string vector)
        {
            string
                sPedazo         = string.Empty,
                sVariable       = string.Empty,
                sValor          = string.Empty,
                sSql            = string.Empty,
                sListaVariables = string.Empty,
                sListaValores   = string.Empty,
                sCondiciones    = string.Empty,
                sTabla          = string.Empty,
                sBaseDatos      = string.Empty;
    
            while (vector.Length > 0) {
                sPedazo = vector.Substring(0,vector.IndexOf(']') + 1);
                if (sPedazo.Substring(1,1).Equals("$")) {
                    sVariable = sPedazo.Substring(2,sPedazo.IndexOf(':') - 3);
                    sValor    = sPedazo.Substring(sPedazo.IndexOf(':') + 1,
                                                        sPedazo.Length - sPedazo.IndexOf(':') - 2);
                    if (sVariable.Equals("_tabla"))
                        sTabla = sValor;
                    else if (sVariable.Equals("_baseDatos"))
                        sBaseDatos = sValor;
                    else if (sVariable.Substring(0,3).Equals("PK_"))
                        sCondiciones += (sCondiciones.Length > 0 ? " AND " : "") +
                                        sVariable.Substring(3,sVariable.Length - 3) + "='" + sValor + "'";
                }
                vector = vector.Substring(vector.IndexOf(']') + 1,vector.Length - vector.IndexOf(']') - 1);
            }
            sSql = "DELETE FROM " + sBaseDatos + ".dbo." + sTabla +
                    " WHERE " + sCondiciones;
            using (Transactor_DAL pdal = Transactor_DAL.instance(dbcode))
                return pdal.ejecutaConsultaGenerada(sSql);
        }

        #endregion
    }
}
