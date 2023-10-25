<!-- %@ Import Namespace="sfc" % -->
<!-- %@ Import Namespace="sfc.BL" % -->
<!-- %@ Import Namespace="sfc.BO" % -->

<!-- #include file="~/core.aspx" -->
<script runat="server" language="C#">


protected void generate_caratula()
{
/*	var batchini= values.get("cbatini");
	var prodini	= values.get("cprdini");
	var lineini	= values.get("clinini");
	var partini	= values.get("cprtini");
	var batchfin= values.get("cbatfin");
	var prodfin	= values.get("cprdfin");
	var linefin	= values.get("clinfin");
	var partfin	= values.get("cprtfin");
	var location= values.get("clocini");

	var maxweeks= values.getint("cmaxwks");
	_check_batch_ranges(batchini, batchfin, maxweeks, 1);

	string target_file;
	string file;
	generate_file("temp", file, target_file);

	CUTF16File obj_file;
	obj_file.Open(target_file, CFile::modeWrite | CFile::modeCreate);
	write_header(obj_file);

///---

	// we get the html formats -------------------------------------------------------------------------------------
	defvartempl("genbtrnghdr.txt", hdrformat); 
	defvartempl("007data.txt", dataformat); 
	defvartempl("010break.txt", breakformat); 
	defvartempl("010end.txt", endformat); 

	getconnectionx(con, obj);

	cCommand command(_basics);

	CString extraqueryfq = _T(" and location='PKG'"); // si es global siempre buscaremos la calidad de empaque
	CString extraqueryrj;
	if(location.IsEmpty() == false)
	{
		command.Format(_T("select object_id from t_qc_insp_types with (nolock) where object_id='%s'"), location);
		con.execute(command, obj);
		ensure(obj.IsEOF(), _T("loc_not_configured"));
		CString loc = obj.get(0);
		extraqueryfq.Format(_T(" and location='%s'"), loc);
		extraqueryrj.Format(_T(" and location='%s'"), loc);
	}

	//       def            base      qty
	map<int, map<CString, long> > data;
	//      base        dummy
	map<CString, int> bases;

	bases[_T("total")] = 1;

	command.Format(	_T("select base, location, qty_base ")
					_T("from vw_batch_detail_by_base_defs with (nolock) ")
					_T("where (batch between '%s' and '%s')  AND (prod_code between '%s' and '%s') AND ")
					_T("(line_id between '%s' and '%s') AND (part between '%s' and '%s') %s and reason_code=0"), 
					batchini, batchfin, prodini, prodfin, lineini, linefin, partini, partfin, extraqueryfq);
	con.execute(command, obj);
	for(;!obj.IsEOF(); obj.MoveNext())
	{
		CString base = obj.get(0);
		CString dep =  obj.get(1);
		int total = obj.getint(2);

		bases[base] = 1;

		data[0][base] += total; // calidad
		data[0][_T("total")] += total; // calidad
		data[-2][base] += total; // llenado
		data[-2][_T("total")] += total; // llenado
	}
	command.Format(	_T("select base, location, reason_code, qty_base ")
					_T("from vw_batch_detail_by_base_defs with (nolock) ")
					_T("where (batch between '%s' and '%s')  AND (prod_code between '%s' and '%s') AND ")
					_T("(line_id between '%s' and '%s') AND (part between '%s' and '%s') %s and reason_code != 0"), 
					batchini, batchfin, prodini, prodfin, lineini, linefin, partini, partfin, extraqueryrj);
	con.execute(command, obj);
	for(;!obj.IsEOF(); obj.MoveNext())
	{
		CString base = obj.get(0);
		CString dep =  obj.get(1);
		int defect = obj.getint(2);
		int total = obj.getint(3);

		bases[base] = 1;

		data[defect][base] += total;
		data[defect][_T("total")] += total;

		data[-1][base] += total; // defects
		data[-1][_T("total")] += total; // defects
		data[-2][base] += total; // llenado
		data[-2][_T("total")] += total; // llenado
	}

	CString ayuda;
	CString helper;
	// print header ---------------------------------------------------------------------------------------------
	ayuda.Format(	_T("batch: [%s:%s), producto: [%s:%s), linea: [%s:%s), part: [%s:%s)"), 
					batchini, batchfin, prodini, prodfin, lineini, linefin, partini, partfin);
	helper.Format(hdrformat, _T("caratula"), 
							_basics.get(ZUSERID, ZUSERIDLEN), 
							COleDateTime::GetCurrentTime().Format(_T("%Y-%m-%d %H:%M:%S")), 
							_basics.get(ZMACNAM, ZMACNAMLEN),
							_T("report : |") + _params.get(_T("_object")) + 
							_T("| caratula, reporte desglozado por bases"),
							ayuda);
	obj_file.WriteString(helper);
	// print header ---------------------------------------------------------------------------------------------

	// get the font size
	int basescount = bases.size();
	int fontpor = 60;
	int widthcell = 50;
	if(basescount >= 5)
	{
		fontpor = (52 / basescount) * 10;
		widthcell = ((double)(40 / basescount)) * 10;
	}
	if(fontpor > 60) fontpor = 60; //por si se pasa
	if(fontpor < 35) fontpor = 35; //por si se pasa
	if(widthcell > 50) widthcell = 50; //por si se pasa
	if(widthcell < 25) widthcell = 25; //por si se pasa
	// get the font size

	// get the defect's description --------------------------------------------------------------------------
	map<int, CString> descriptions;
	command.Format(_T("select defect_id, short_description from t_qs_defects with (nolock) "));
	con.execute(command, obj);
	for(;!obj.IsEOF(); obj.MoveNext())
		descriptions.insert(map<int, CString>::value_type(obj.getint(_T("defect_id")), 
														obj.get(_T("short_description"))));
	// get the defect's description --------------------------------------------------------------------------

	// + get fields---------------------------------------------------------------------------------------------
	CString fields;
	map<CString, int>::iterator iterbases = bases.begin();
	int middle = bases.size() / 2;
	for(int i = 1; iterbases != bases.end(); ++iterbases, ++i)
	{
		CString base = (*iterbases ).first;
		TCHAR align[16];
		_tcscpy_s(align, i > middle ? _T("right") : _T("left"));
		ayuda.Format(dataformat, align, widthcell, align, fontpor, base, align, widthcell, align, fontpor, _T("por."));
		fields += ayuda;
	}
	// - get fields---------------------------------------------------------------------------------------------
	// print fields---------------------------------------------------------------------------------------------
	ayuda.Format(	_T("<hr><TABLE BORDER=0 CELLSPACING=0 CELLPADDING=0><TR HEIGHT=8 >\r\n")		
					_T("<td valign='left' width='90'><div align='left'><font face='Lucida Console' style='font-size: %d%%'>defectos</font></div></td>\r\n")
					_T("%s</TR></TABLE>\r\n<hr>\r\n"),	fontpor, fields);
	obj_file.WriteString(ayuda);
	// print fields---------------------------------------------------------------------------------------------

	// + get data---------------------------------------------------------------------------------------------
	map<int, map<CString, long> >::iterator iter = data.begin();
	for(; iter != data.end(); ++iter)
	{
		int defect = (*iter).first;
		if(defect == -1 || defect == -2) continue;
		gen_caratula_line(dataformat, data, bases, defect, ayuda, fontpor, descriptions[defect], widthcell);
		obj_file.WriteString(ayuda);
		// - print data---------------------------------------------------------------------------------------------
	}
	obj_file.WriteString(_T("<br>"));
	// - get data---------------------------------------------------------------------------------------------
	gen_caratula_line(dataformat, data, bases, -1, ayuda, fontpor, _T("rechazos"), widthcell);
	obj_file.WriteString(ayuda);
	// - print data---------------------------------------------------------------------------------------------
	// - get data---------------------------------------------------------------------------------------------
	gen_caratula_line(dataformat, data, bases, 0, ayuda, fontpor, _T("1ra Cal."), widthcell);
	obj_file.WriteString(ayuda);
	// - print data---------------------------------------------------------------------------------------------
	// - get data---------------------------------------------------------------------------------------------
	gen_caratula_line(dataformat, data, bases, -2, ayuda, fontpor, _T("Llenado"), widthcell);
	obj_file.WriteString(ayuda);
	// - print data---------------------------------------------------------------------------------------------

	ayuda.Format(_T("<br><font face='Lucida Console' style='font-size: 50%%'>primera calidad: %d</font><br>\r\n"), data[0][_T("total")]);
	obj_file.WriteString(ayuda);
	ayuda.Format(_T("<font face='Lucida Console' style='font-size: 50%%'>rechazos: %d</font><br>\r\n"), data[-1][_T("total")]);
	obj_file.WriteString(ayuda);
	ayuda.Format(_T("<font face='Lucida Console' style='font-size: 50%%'>gran total: %d</font><br>\r\n"), data[-2][_T("total")]);
	obj_file.WriteString(ayuda);
	ayuda.Format(_T("<font face='Lucida Console' style='font-size: 50%%'>yield global: %.2f%%</font><br>\r\n"), 
				data[-2][_T("total")] == 0 ? 0.0 : (double)(((double)data[0][_T("total")]/(double)data[-2][_T("total")])*100));
	obj_file.WriteString(ayuda);

	// < sacar CPM ------------------------------------------------------------
	int l_MaxF=0;
	int l_MaxB=0;
	float l_CPMF=0.0;
	float l_CPMB=0.0;
	command.Format(	_T("select mouldfb, total from tdtlml with (nolock) ")
					_T("where (DtlID BETWEEN '%s' AND '%s') AND (pcc BETWEEN '%s' AND '%s') AND (Line BETWEEN '%s' AND '%s') AND (Part BETWEEN '%s' AND '%s')"), 
					batchini, batchfin, prodini, prodfin, lineini, linefin, partini, partfin);
	con.execute(command, obj);
	for(; !obj.IsEOF(); obj.MoveNext())
	{
		int total = obj.getint(_T("total"));
		if(obj.get(_T("MouldFB")) == _T("F"))
			l_MaxF += total;
		else
			if(obj.get(_T("MouldFB")) == _T("B"))
				l_MaxB += total;
	}
	if(l_MaxF!=0) l_CPMF = (float)data[-2][_T("total")]/l_MaxF;
	if(l_MaxB!=0) l_CPMB = (float)data[-2][_T("total")]/l_MaxB;
	// > ----------------------------------------------------------------------

	ayuda.Format(_T("<br><font face='Lucida Console' style='font-size: 50%%'>frentes: %d</font><br>\r\n"),l_MaxF);
	obj_file.WriteString(ayuda);
	ayuda.Format(_T("<font face='Lucida Console' style='font-size: 50%%'>CPM: %.2f</font><br>\r\n"), l_CPMF);
	obj_file.WriteString(ayuda);
	ayuda.Format(_T("<font face='Lucida Console' style='font-size: 50%%'>bases: %d</font><br>\r\n"), l_MaxB);
	obj_file.WriteString(ayuda);
	ayuda.Format(_T("<font face='Lucida Console' style='font-size: 50%%'>CPM: %.2f</font><br>\r\n"), l_CPMB);
	obj_file.WriteString(ayuda);

	// - saber los lotes envueltos-----------------------------------------------------------------------------------------------
	obj_file.WriteString(_T("<br>lotes envueltos<br><hr>\r\n"));

	getconnectionx(statusdb, sta);

	command.Format(	_T("select hdrid, pcc, line, part, status from thdr with (nolock) ")
					_T("where (hdrid between '%s' and '%s') and (pcc between '%s' and '%s') and (line between '%s' and '%s') and (part between '%s' and '%s')"), 
					batchini, batchfin, prodini, prodfin, lineini, linefin, partini, partfin);
	con.execute(command, obj);
	const int maxperline = 4;
	int index = 0;
	helper.Empty();
	for(; !obj.IsEOF(); obj.MoveNext())
	{
		CString statusdesc = _T("?");
		command.Format(_T("select description from t_status with (nolock) where status_id = %d"), obj.getint(_T("status")));
		statusdb.execute(command, sta);
		if(sta.IsEOF() == false) statusdesc = sta.get(_T("description"));
		ayuda.Format(	_T("<td valign='left' width='300'><div align='left'><font face='Lucida Console' style='font-size: 45%%'>%s:%s:%s:%s-%s</font></div></td>\r\n"),
						obj.get(_T("hdrid")), obj.get(_T("pcc")), obj.get(_T("line")), obj.get(_T("part")), statusdesc);
		helper += ayuda;
		if(index++ == maxperline - 1)
		{
			ayuda.Format(_T("<TABLE BORDER=0 CELLSPACING=0 CELLPADDING=0><TR HEIGHT=8 >\r\n")		
									_T("%s</TR></TABLE>\r\n"),	helper);
			obj_file.WriteString(ayuda);
			index = 0;
			helper.Empty();
		}
	}
	if(index != 0)
	{
		ayuda.Format(_T("<TABLE BORDER=0 CELLSPACING=0 CELLPADDING=0><TR HEIGHT=8 >\r\n")		
								_T("%s</TR></TABLE>\r\n"),	helper);
		obj_file.WriteString(ayuda);
	}
	obj_file.Close();
	mark_as_download_and_seeable(_T("temp"), file, _params.get(_T("_action")));
    */
}

protected void generate_m1()
{
/*	CString semana		= _params.get(_T("csemini"));
	CString anio		= _params.get(_T("canoini"));
	CString workbook	= _params.get(_T("cbookini"));
	CString bookid		= _params.get(_T("cbookid"));

	require(semana.IsEmpty(),	_T("inc_dat_week"));
	require(workbook.IsEmpty(), _T("inc_dat_workook"));

	if(anio.IsEmpty()) anio = COleDateTime::GetCurrentTime().Format(_T("%Y")).Mid(3,1);

	cCommand command(_basics);
	CString cell;

	int l_iSemanaIni = 1;
	int l_iSemanaFin = _tstoi(semana);

	CString file;
	CString target_file = generate_destiny(file, workbook);

	defchar(sys, 4);
	_basics.get(ZSYSTEM, sys, ZSYSTEMLEN, ZSYSTEMMAX);
	defchar(lang, ZLANGUAMAX+1);
	_basics.get(ZLANGUA, lang, ZLANGUALEN, ZLANGUAMAX);

	getconnectionx(con, obj);

	command.Format(	_T("select parameters from tworkbooks with (nolock) ")
					_T("where shortdesc='%s' and system='%s' and language_id='%s'"),
					workbook, sys, lang);
	con.execute(command, obj);
	if(obj.IsEOF())
	{
		command.Format(	_T("select parameters from tworkbooks with (nolock) ")
						_T("where shortdesc='%s' and system='%s' and language_id='%s'"),
						bookid, sys, lang);
		con.execute(command, obj);
		ensure(obj.IsEOF(), _T("workbook_range_not_exist"));
	}

	CParameters prms = obj.get(_T("parameters"));

	CString Rango1Ini = prms.get(_T("r1i"));
	CString Rango1Fin = prms.get(_T("r1f"));
	CString Rango2Ini = prms.get(_T("r2i"));
	CString Rango2Fin = prms.get(_T("r2f"));

	ensure(Rango1Ini.IsEmpty(), _T("wrong_range_ini_1"));
	ensure(Rango1Fin.IsEmpty(), _T("wrong_range_fin_1"));
	ensure(Rango2Ini.IsEmpty(), _T("wrong_range_ini_2"));
	ensure(Rango2Fin.IsEmpty(), _T("wrong_range_fin_2"));

	if(_tstoi(semana)>13)
	{
		l_iSemanaIni = _tstoi(semana) - 12;
		command.Format(	_T("select * from t_rp_trimestral with (nolock) ")
						_T(" where ((line between '%s' and '%s') or (line between '%s' and '%s'))  and ")
						_T("(week between %d and %d) and year=%s order by line,year,week"),
						Rango1Ini,Rango1Fin,Rango2Ini,Rango2Fin,l_iSemanaIni,l_iSemanaFin,anio);
	}
	else
		command.Format(	_T("select * from t_rp_trimestral with (nolock) ")
						_T(" where (((line between '%s' and '%s') or (line between '%s' and '%s')) and ")
						_T("(week between %d and %d) and year=%d) or (((line between '%s' and '%s') or (line between '%s' and '%s')) and ")
						_T("(week between %d and %d) and year=%d) order by line,year,week"),
						Rango1Ini,Rango1Fin,Rango2Ini,Rango2Fin,l_iSemanaIni,l_iSemanaFin,_tstoi(anio),
						Rango1Ini,Rango1Fin,Rango2Ini,Rango2Fin,52-(12-_tstoi(semana)),52,_tstoi(anio)-1);

	con.execute(command, obj);

	{
	cMroExcel book(target_file);

	int l_iLetter = 66;
	CString l_strLineAnt = _T("");
	CString sheet;
	for(;!obj.IsEOF();obj.MoveNext())
	{
		if(l_strLineAnt != obj.get(_T("line"))) l_iLetter = 66;

		try
		{
			sheet.Format(_T("line_%s"), obj.get(_T("line")));
			sheet.TrimRight();
			book.workonsheet(sheet);
		}
		catch(...)
		{
			continue; // no encontro el workbook
		}

		CString helper;

		cell.Format(_T("%c3"),l_iLetter);
		book.setvalue(cell, ((short)obj.getlong(_T("week"))));

		cell.Format(_T("%c4"),l_iLetter);
		helper.Format(_T("%f%%"),obj.getdouble(_T("cump")));
		book.setvalue(cell, helper);

		cell.Format(_T("%c5"),l_iLetter);
		helper.Format(_T("%f%%"),obj.getdouble(_T("abs_x_totales")));
		book.setvalue(cell, helper);

		cell.Format(_T("%c7"),l_iLetter);
		book.setvalue(cell, ((float)obj.getlong(_T("qty"))));

		cell.Format(_T("%c8"),l_iLetter++);
		book.setvalue(cell, ((float)obj.getlong(_T("planned"))));

		l_strLineAnt = obj.get(_T("line"));
	}

	book.save();
	}
	mark_as_download_and_seeable(_T("temp"), target_file, file, _params.get(_T("_action")));
	*/
}

protected void generate_m3()
{
/*	CString semana   = _params.get(_T("csemini"));
	CString anio     = _params.get(_T("canoini"));
	CString workbook = _params.get(_T("cbookini"));
	CString bookid   = _params.get(_T("cbookid"));

	require(semana.IsEmpty(), _T("inc_dat_week"));
	require(workbook.IsEmpty(), _T("inc_dat_workook"));

	if(anio.IsEmpty()) anio = COleDateTime::GetCurrentTime().Format(_T("%Y")).Mid(3,1);

	CString file;
	CString target_file = generate_destiny(file, workbook);

	defchar(sys, 4);
	_basics.get(ZSYSTEM, sys, ZSYSTEMLEN, ZSYSTEMMAX);
	defchar(lang, ZLANGUAMAX+1);
	_basics.get(ZLANGUA, lang, ZLANGUALEN, ZLANGUAMAX);

	getconnectionx(con, obj);

	cCommand command(_basics);

	CString cell;

	int l_iSemanaIni = 1;
	int l_iSemanaFin = _tstoi(semana);

	command.Format(	_T("select parameters from tworkbooks with (nolock) ")
					_T("where shortdesc='%s' and system='%s' and language_id='%s'"),
					workbook, sys, lang);
	con.execute(command, obj);
	if(obj.IsEOF())
	{
		command.Format(	_T("select parameters from tworkbooks with (nolock) ")
						_T("where shortdesc='%s' and system='%s' and language_id='%s'"),
						bookid, sys, lang);
		con.execute(command, obj);
		ensure(obj.IsEOF(), get_error(_T("workbook_range_not_exist")));
	}

	CParameters prms = obj.get(_T("parameters"));

	CString Rango1Ini = prms.get(_T("r1i"));
	CString Rango1Fin = prms.get(_T("r1f"));
	CString Rango2Ini = prms.get(_T("r2i"));
	CString Rango2Fin = prms.get(_T("r2f"));

	ensure(Rango1Ini.IsEmpty(), _T("wrong_range_ini_1"));
	ensure(Rango1Fin.IsEmpty(), _T("wrong_range_fin_1"));

	if(_tstoi(semana)>13)
	{
		l_iSemanaIni = _tstoi(semana) - 12;
		command.Format(	_T("select * from t_rp_trimestral_m3 with (nolock) ")
						_T("where ((bsu between '%s' and '%s') or (bsu between '%s' and '%s'))  and (week between %d and %d) and year=%s ")
						_T(" order by BSU, year, week "),
						Rango1Ini,Rango1Fin,Rango2Ini,Rango2Fin,l_iSemanaIni,l_iSemanaFin,anio);
	}
	else
		command.Format(	_T("select * from t_rp_trimestral_m3 with (nolock) ")
						_T("where (((bsu between '%s' and '%s') or (bsu between '%s' and '%s')) and (week between %d and %d) and year=%d) or ")
						_T("(((bsu between '%s' and '%s') or (bsu between '%s' and '%s')) and (week between %d and %d) and year=%d) ")
						_T(" order by BSU, year, week "),
						Rango1Ini,Rango1Fin,Rango2Ini,Rango2Fin,l_iSemanaIni,l_iSemanaFin,_tstoi(anio),
						Rango1Ini,Rango1Fin,Rango2Ini,Rango2Fin,52-(12-_tstoi(semana)),52,_tstoi(anio)-1);

	con.execute(command, obj);

	{
	cMroExcel book(target_file);

	int l_iLetter = 66;
	CString sheet;
	CString l_strLineAnt = _T("");
	for(;!obj.IsEOF();obj.MoveNext())
	{
		if(l_strLineAnt != obj.get(_T("BSU"))) l_iLetter = 66;

		try
		{
			sheet.Format(_T("bsu_%s"), obj.get(_T("BSU")));
			sheet.TrimRight();
			book.workonsheet(sheet);
		}
		catch(...)
		{
			continue; // no encontro el workbook
		}

		CString helper;

		cell.Format(_T("%c3"),l_iLetter);
		book.setvalue(cell, ((short)obj.getlong(_T("week"))));

		cell.Format(_T("%c4"),l_iLetter);
		helper.Format(_T("%f%%"),obj.getdouble(_T("cump")));
		book.setvalue(cell, helper);

		cell.Format(_T("%c5"),l_iLetter);
		helper.Format(_T("%f%%"),obj.getdouble(_T("abs_x_totales")));
		book.setvalue(cell, helper);

		cell.Format(_T("%c7"),l_iLetter);
		book.setvalue(cell, ((float)obj.getlong(_T("qty"))));

		cell.Format(_T("%c8"),l_iLetter++);
		book.setvalue(cell, ((float)obj.getlong(_T("planned"))));

		l_strLineAnt = obj.get(_T("BSU"));
	}

	book.save();
	}
	mark_as_download_and_seeable(_T("temp"), target_file, file, _params.get(_T("_action")));
*/
}
</script>