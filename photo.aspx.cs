using System;
using System.Data;
using System.IO;
using System.Web.UI.WebControls;
using Microsoft.Azure;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using System.Collections.Generic;
using System.Data.SqlClient;

public partial class photo : App_Function

{
	private int rtn;
	private Boolean sqlrtn;
	private string businessName = "初回訪問ガイド";
	private string displayName = "photo";
	protected string title = "画像撮影";


	protected void Page_Load(object sender, EventArgs e)
	{
		checkSession(0);

		if (!IsPostBack)
		{
			try
			{
				bool ret = logLong(2, businessName, displayName, "起動:", "", "", "");

				//string flg = Request.QueryString["flg"];
				string flg = getQueryString("flg");
				hdnFlg.Value = flg;
				//string exc = Request.QueryString["exc"];
				string exc = getQueryString("exc");
				hdnExclusive.Value = exc;
				//string patientId = Request.QueryString["patientId"];
				string patientId = getQueryString("patientId");
				hdnPatientId.Value = patientId;
				hdnHospId.Value = getSession("HOSP_ID");

				pmif5.SetOrigin(this);

				rtn = patientInfo();
				rtn = imageSet();

				js("fncLoad('" + flg + "')");
                js("setRests()");
            }
			catch (Exception ex)
			{
				logLong(4, businessName, displayName, "Loadエラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			}
		}
		createTable();
	}

	protected void btnCmn_Click(object sender, EventArgs e)
	{

	}

	/// <summary>
	/// 患者情報取得
	/// </summary>
	/// <returns></returns>
	private int patientInfo()
	{
		try
		{
			string sqlString = "";
			string patientName = "";
			sqlString = "select patient_name from tbl_patient ";
			sqlString += "where hosp_id = '" + getSession("HOSP_ID") + "'";
			sqlString += "  and patient_id = '" + hdnPatientId.Value + "'";

			patientName = sqlSelect(sqlString, "patient_name");
			lbPatientName.Text = patientName;
			hdnPatientName.Value= patientName;

			return 0;
		}
		catch (Exception ex)
		{
			bool ret = logLong(4, businessName, displayName, "患者情報取得エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return -1;
		}
	}




	/// <summary>
	/// 保存ボタン
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void btnSave_Click(object sender, EventArgs e)
	{
		try
		{
			logLong(3, businessName, displayName, "保存ボタン押下", hdnPatientId.Value, hdnPatientName.Value, "");

			string userId = "";
			rtn = exclusiveCheck("firstVisit", hdnPatientId.Value, "", ref userId);
			if (rtn == 0)
			{
				if (userId != getSession("LOGIN_ID") && hdnExclusive.Value == "1")
				{
					//自分が排他していないので、編集権を取られている。
					js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の取り込みができませんでした。<br />編集中に別の端末から編集作業されました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"0\",\"\")\');");
					return;
				}
				else
				{
					//自分が排他してるので保存可能
				}
			}
			else
			{
				js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の取り込みに失敗しました。<br />管理者に連絡してください。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\", 'ＯＫ', 'alertErr()');");
				return;
			}

			if (hdnImageKbn.Value == "1")
			{
				//カメラ
				string imageName = "";
				rtn = save(ref imageName);
				imageName = getConst("endPoint") + getConst("container") + "/" + imageName;
				if (rtn == 0)
				{
					if (hdnCloseFlg.Value == "0")
					{
						//js("alertmsg('情報', '<article>画像を取り込みました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"2\",\"\")\');");
						js("alertmsg_img('情報', '<article>撮影した画像を保存しました。</ article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"2\",\"\")\','" + imageName + "');");
					}
					else
					{
						//js("alertmsg('情報', '<article>画像を取り込みました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"0\",\"\")\');");
						js("alertmsg_img('情報', '<article>撮影した画像を保存しました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"0\",\"\")\','" + imageName + "');");
					}
				}
				else
				{
					logLong(4, businessName, displayName, "カメラ保存処理エラー", hdnPatientId.Value, hdnPatientName.Value, "");
					js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の取り込みに失敗しました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"9\",\"\")\');");
					return;
				}
				js("fncCameraEnd()");
			}

			panelImage.Controls.Clear();
			imageSet();
			createTable();
			//js("fncClose();");

		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "保存ボタン押下エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
		}
	}

	/// <summary>
	/// 保存処理
	/// </summary>
	/// <returns></returns>
	private int save(ref string imageName)
	{
		try
		{
			//選択されている患者ID。なければ新規
			string patientId = hdnPatientId.Value;

			string imageData = hdnImageData.Value;
			//string fileName = hdnFileName.Value;
			string fileName = "";

			string photoDate = DateTime.Today.Year.ToString() + "_" + DateTime.Today.Month.ToString() + "_" + DateTime.Today.Day.ToString();
			//DateTime photoDate = DateTime.Now;

			//使い終わったら消しておく
			hdnImageData.Value = "";
			hdnFileName.Value = "";

			//グループ番号の取得
			string sqlString = "";
			string sImageNo = "";
			int iImageNo = 0;

			sqlString = "select max(image_no) as image_no from tbl_patient_image ";
			sqlString += "where hosp_id = '" + getSession("HOSP_ID") + "'";
			sqlString += "  and patient_id = '" + hdnPatientId.Value + "' ";
			sqlString += "  and image_kbn = '1' ";
			sImageNo = sqlSelect(sqlString, "image_no");

			if (string.IsNullOrEmpty(sImageNo)) {
				iImageNo = 1;
			}else {
				iImageNo = int.Parse(sImageNo) + 1;
			}

			fileName = photoDate + "_" + iImageNo.ToString() + ".jpg";

			//string folderPath = getConst("Server") + getConst("ImageFile") + hdnHospId.Value + "\\" + hdnPatientId.Value + "\\" + photoDate + "\\" + iImageNo;
			//string folderPathDB = getConst("ImageFile") + hdnHospId.Value + "\\" + hdnPatientId.Value + "\\" + photoDate + "\\" + iImageNo + "\\";
			string folderPath =  hdnHospId.Value + "\\" + hdnPatientId.Value + "\\" + photoDate + "\\" + iImageNo;
			string folderPathDB =  hdnHospId.Value + "\\" + hdnPatientId.Value + "\\" + photoDate + "\\" + iImageNo + "\\";
			string filePath = folderPath + "\\" + fileName;
			string fileExt = (Path.GetExtension(filePath)).ToLower();




			//Blobに保存
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
			CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
			CloudBlobContainer container = blobClient.GetContainerReference(getConst("container"));
			CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);


			//同じファイル名があった場合は名前を変える
			if (blobExist(blockBlob))
			{
				//string fileExt = (System.IO.Path.GetExtension(filePath)).ToLower();
				string fileNameNEW = System.IO.Path.GetFileNameWithoutExtension(filePath);
				string strFilePathNEW;
				int i = 0;
				do
				{
					i += 1;
					strFilePathNEW = folderPath + "\\" + fileNameNEW + "_" + String.Format("{0:0000}", i) + fileExt;
					blockBlob = container.GetBlockBlobReference(strFilePathNEW);
				} while (blobExist(blockBlob) == true);
				fileName = fileNameNEW + "_" + String.Format("{0:0000}", i) + fileExt;
			}







			////フォルダが無い場合は作る
			//if (!System.IO.Directory.Exists(folderPath)) {
			//	System.IO.Directory.CreateDirectory(folderPath);
			//}

			//folderPath += "\\";

			////同じファイル名があった場合は名前を変える
			//if (System.IO.File.Exists(filePath)) {
			//	string fileNameNEW = System.IO.Path.GetFileNameWithoutExtension(filePath);
			//	string strFilePathNEW;
			//	int i = 0;
			//	do {
			//		i += 1;
			//		strFilePathNEW = folderPath + fileNameNEW + "_" + String.Format("{0:0000}", i) + fileExt;
			//	} while (System.IO.File.Exists(strFilePathNEW) == true);
			//	fileName = fileNameNEW + "_" + String.Format("{0:0000}", i) + fileExt;
			//}

			//jpg保存
			//FileStream fs = new FileStream(string.Format("{0}{1}", folderPath, fileName), FileMode.Create, FileAccess.Write);
			//BinaryWriter bw = new BinaryWriter(fs);
			//byte[] data = Convert.FromBase64String(imageData);
			//imageData = null;

			try
			{

				Stream s = new MemoryStream(Convert.FromBase64String(imageData));

				//bw.Write(data, 0, data.Length);
				blockBlob.UploadFromStream(s);
				rtn = fncAddImage((folderPathDB + fileName), iImageNo);

				imageName = (folderPathDB + fileName).Replace('\\', '/');
			}
			catch (Exception e)
			{
				bool ret = logLong(4, businessName, displayName, "画像登録２エラー:" + e.Message, hdnPatientId.Value, hdnPatientName.Value, "");
				rtn = -1;
			}

			//data = null;
			//bw.Close();

			return 0;
		}
		catch (Exception ex)
		{
			bool ret = logLong(4, businessName, displayName, "画像登録エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return -1;
		}
	}

	/// <summary>
	/// 登録処理
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="imageNo"></param>
	/// <returns></returns>
	private int fncAddImage(string fileName, int imageNo)
	{
		try
		{
			//登録日
			//DateTime photoDate;
			//registDate = DateSerial(ddlYear.SelectedValue, ddlMonth.SelectedValue, ddlDay.SelectedValue)

			//Dim fileExt As String = (System.IO.Path.GetExtension(fileName)).ToLower


			//新規で登録する
			string sqlString = "";

			sqlString = "insert into tbl_patient_image ";
			sqlString += "(hosp_id, ";
			sqlString += "patient_id,";
			sqlString += "image_no,";
			sqlString += "image_kbn,";
			sqlString += "mask,";
			sqlString += "regist_date,";
			sqlString += "photo_date,";
			sqlString += "title,";
			sqlString += "image_path,";
			sqlString += "edit_date,";
			sqlString += "edit_user) ";
			sqlString += "values('" + getSession("HOSP_ID") + "',";
			sqlString += "'" + hdnPatientId.Value + "',";
			sqlString += imageNo + ",";
			sqlString += "'1',";
			sqlString += "'0',";
			sqlString += "'" + DateTime.Now + "',";
			sqlString += "'" + DateTime.Now + "',";
			sqlString += "'" + sqlTabooChar(txtTitle.Text) + "',";
			sqlString += "'" + sqlTabooChar(fileName.Replace('\\', '/')) + "',";
			sqlString += "'" + DateTime.Now + "',";
			sqlString += "'" + getSession("LOGIN_ID") + "')";


			sqlrtn = sqlInsert(sqlString);
			if (!sqlrtn)
			{
				logLong(4, businessName, displayName, "画像保存エラー:" + sqlString, hdnPatientId.Value, hdnPatientName.Value, "");
				return -1;
			}

			return 0;
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "画像保存エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return -1;
		}
	}

	/// <summary>
	/// 保存ボタン
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void btnSave2_Click(object sender, EventArgs e)
	{
		try
		{
			logLong(3, businessName, displayName, "保存ボタン押下", hdnPatientId.Value, hdnPatientName.Value, "");

			string userId = "";
			rtn = exclusiveCheck("firstVisit", hdnPatientId.Value, "", ref userId);
			if (rtn == 0)
			{
				if (userId != getSession("LOGIN_ID") && hdnExclusive.Value == "1")
				{
					//自分が排他していないので、編集権を取られている。
					js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の取り込みができませんでした。<br />編集中に別の端末から編集作業されました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"0\",\"\")\');");
					return;
				}
				else
				{
					//自分が排他してるので保存可能
				}
			}
			else
			{
				js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の取り込みに失敗しました。<br />管理者に連絡してください。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', 'alertErr()');");
				return;
			}

			if (hdnImageKbn.Value == "2")
			{
				//選択
				rtn = save2();
				jsl(imgFile.ImageUrl);
				if (rtn == 0)
				{
					if (hdnCloseFlg.Value == "0")
					{
						//js("alertmsg('情報', '<article>画像を取り込みました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"2\",\"\")\');");
						js("alertmsg_img('情報', '<article>画像を取り込みました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"2\",\"\")\','" + imgFile.ImageUrl + "');");
					}
					else
					{
						//js("alertmsg('情報', '<article>画像を取り込みました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"0\",\"\")\');");
						js("alertmsg_img('情報', '<article>画像を取り込みました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"0\",\"\")\','" + imgFile.ImageUrl + "');");
					}
				}
				else
				{
					logLong(4, businessName, displayName, "選択保存処理エラー", hdnPatientId.Value, hdnPatientName.Value, "");
					js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の取り込みに失敗しました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"9\",\"\")\');");
					return;
				}
			}

			//使い終わったら消しておく
			txtTitle2.Text = "";
			hdnFilePath.Value = "";
			hdnPhotoDate.Value = "";
			imgFile.ImageUrl = hdnFilePath.Value;


			panelImage.Controls.Clear();
			imageSet();
			createTable();
			//js("fncClose();");

		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "保存ボタン押下エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
		}
	}

	/// <summary>
	/// 保存処理
	/// </summary>
	/// <returns></returns>
	private int save2()
	{
		try
		{
			//選択されている患者ID。なければ新規
			string patientId = hdnPatientId.Value;

			if (string.IsNullOrEmpty(hdnFilePath.Value))
			{
				return 100;
			}

			string photoDate = DateTime.Today.Year.ToString() + "_" + DateTime.Today.Month.ToString() + "_" + DateTime.Today.Day.ToString();

			//グループ番号の取得
			string sqlString = "";
			string sImageNo = "";
			int iImageNo = 0;

			sqlString = "select max(image_no) as image_no from tbl_patient_image ";
			sqlString += "where hosp_id = '" + getSession("HOSP_ID") + "'";
			sqlString += "  and patient_id = '" + hdnPatientId.Value + "' ";
			sqlString += "  and image_kbn = '1' ";
			sImageNo = sqlSelect(sqlString, "image_no");

			if (string.IsNullOrEmpty(sImageNo))
			{
				iImageNo = 1;
			}
			else
			{
				iImageNo = int.Parse(sImageNo) + 1;
			}

			sqlString = "insert into tbl_patient_image ";
			sqlString += "(hosp_id, ";
			sqlString += "patient_id,";
			sqlString += "image_no,";
			sqlString += "image_kbn,";
			sqlString += "mask,";
			sqlString += "regist_date,";
			sqlString += "photo_date,";
			sqlString += "title,";
			sqlString += "image_path,";
			sqlString += "edit_date,";
			sqlString += "edit_user) ";
			sqlString += "values('" + getSession("HOSP_ID") + "',";
			sqlString += "'" + hdnPatientId.Value + "',";
			sqlString += iImageNo + ",";
			sqlString += "'1',";
			sqlString += "'0',";
			sqlString += "'" + DateTime.Now + "',";
			sqlString += "'" + sqlTabooChar(hdnPhotoDate.Value) + "',";
			sqlString += "'" + sqlTabooChar(txtTitle2.Text) + "',";
			sqlString += "'" + sqlTabooChar(hdnFilePath.Value) + "',";
			sqlString += "'" + DateTime.Now + "',";
			sqlString += "'" + getSession("LOGIN_ID") + "')";

			sqlrtn = sqlInsert(sqlString);
			if (!sqlrtn)
			{
				logLong(4, businessName, displayName, "画像保存エラー:" + sqlString, hdnPatientId.Value, hdnPatientName.Value, "");
				return -1;
			}

			return 0;
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "画像登録エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return -1;
		}
	}

	/// <summary>
	/// 検索ボタン
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void btnSearch_Click(object sender, EventArgs e)
	{
		try
		{
			logLong(3, businessName, displayName, "検索ボタン押下", hdnPatientId.Value, hdnPatientName.Value, "");

			rtn = imageSet();
			if (rtn == -1)
			{
				logLong(4, businessName, displayName, "検索処理エラー", hdnPatientId.Value, hdnPatientName.Value, "");

			}
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "検索ボタン押下エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
		}
	}

	/// <summary>
	/// 情報セット
	/// </summary>
	/// <returns></returns>
	private int imageSet()
	{
		try
		{
			//tbl
			DataTable dt = new DataTable();
			string sqlString = "";

			//患者名表示
			sqlString = "select * from tbl_patient_image ";
			sqlString += "where hosp_id = '" + getSession("HOSP_ID") + "'";
			sqlString += "  and patient_id = '" + hdnPatientId.Value + "'";
			sqlString += "  and image_kbn = '1'";
			sqlString += "  and mask = '0' ";
			sqlString += "order by photo_date desc";
			sqlrtn = sqlSelectTable(sqlString, ref dt);

			if (sqlrtn)
			{
			}
			else
			{
				logLong(4, businessName, displayName, "情報セットエラー:" + sqlString, hdnPatientId.Value, hdnPatientName.Value, "");
				return -1;
			}

			ViewState["dt0"] = dt;

			return 0;
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "情報セットエラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return -1;
		}

	}


	public int createTable()
	{
		try
		{
			if (ViewState["dt0"] == null) { return 0; };

			DataTable dt = (DataTable)ViewState["dt0"];
			int cnt = dt.Rows.Count;

			Table tbl1 =  new Table();
			tbl1.CssClass = "tblInfo";
			TableRow[] infoRow = new TableRow[3];

			for (int i = 0; i < cnt; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					infoRow[j] = new TableRow();
					if (j == 0)
					{
						TableCell infoCell = new TableCell();

						HiddenField hdn2 = new HiddenField();
						hdn2.ID = "hdnTitleImg_" + i.ToString();
						hdn2.Value = dt.Rows[i]["title"].ToString();
						infoCell.Controls.Add(hdn2);
						HiddenField hdn3 = new HiddenField();
						hdn3.ID = "hdnImagePathImg_" + i.ToString();

						Image img = new Image();
						//img.ImageUrl = dt.Rows[i]["image_path"].ToString();
						//img.ImageUrl = "https://wyfile01.blob.core.windows.net/wyblobdev01/test/test.jpg";
						if (string.IsNullOrEmpty(dt.Rows[i]["image_path"].ToString()))
						{
							img.ImageUrl = "";
							hdn3.Value = "";
						}
						else
						{
							//Blobに保存
							CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
							CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
							CloudBlobContainer container = blobClient.GetContainerReference(getConst("container"));
							CloudBlockBlob blockBlob = container.GetBlockBlobReference(dt.Rows[i]["image_path"].ToString());

							if (blobExist(blockBlob))
							{
								img.ImageUrl = getConst("endPoint") + getConst("container") + "/" + dt.Rows[i]["image_path"].ToString();
								hdn3.Value = getConst("endPoint") + getConst("container") + "/" + dt.Rows[i]["image_path"].ToString();
							}
							else
							{
								img.ImageUrl = "";
								hdn3.Value = "";
							}
						}

						infoCell.Controls.Add(hdn3);

                        img.Attributes["onclick"] = "fncImage(this)";
						img.CssClass = "imgInfo";
						infoCell.Controls.Add(img);
						infoCell.RowSpan = 3;
						infoCell.CssClass = "btmBorder";
						infoRow[j].Controls.Add(infoCell);


						TableCell infoCell2 = new TableCell();

						Literal li3 = new Literal();
						li3.Text = "<div class='divTitle'>";
						infoCell2.Controls.Add(li3);

						Label lb = new Label();
						lb.Text = "タイトル：";
						infoCell2.Controls.Add(lb);

						Literal li4 = new Literal();
						li4.Text = "</div>";
						infoCell2.Controls.Add(li4);

						infoRow[j].Controls.Add(infoCell2);

						Literal li1 = new Literal();
						li1.Text = "<div class='divRest'>";
						infoCell2.Controls.Add(li1);

						TextBox txt = new TextBox();
                        txt.ID =  "txtTitleList_" + i.ToString() ;
						txt.Text = dt.Rows[i]["title"].ToString();
						txt.MaxLength = 40;
						//txt.Width = 600;
						if (hdnExclusive.Value == "2")
						{
							txt.Enabled = false;
						}
						txt.Attributes["onblur"] = "fncMaxCheck(this)";
						infoCell2.Controls.Add(txt);

						Literal li2 = new Literal();
						li2.Text = "</div>";
						infoCell2.Controls.Add(li2);

						infoRow[j].Controls.Add(infoCell2);


						TableCell infoCell3 = new TableCell();

						HiddenField hdn = new HiddenField();
						hdn.ID = "hdnTitleNo_" + i.ToString();
						hdn.Value = dt.Rows[i]["image_no"].ToString();
                        //infoCell3.CssClass = "fb mgnT";
                        infoCell3.Controls.Add(hdn);



                        if (hdnExclusive.Value != "2")
						{
							Label lb2 = new Label();
							lb2.Text = "更新";
							lb2.Attributes["onclick"] = "fncUpdate('" + i.ToString() + "')";
							lb2.CssClass = "btn btnSizeM";
							infoCell3.Controls.Add(lb2);
							infoRow[j].Controls.Add(infoCell3);
						}

                        //if (hdnExclusive.Value != "2")
                        //{
                        //    Label lb2 = new Label();
                        //    lb2.Text = "ペン";
                        //    lb2.Attributes["onclick"] = "callTouchKey()";
                        //    lb2.CssClass = "btn btnSizeMi";
                        //    infoCell3.Controls.Add(lb2);
                        //    infoRow[j].Controls.Add(infoCell3);
                        //    //imgFile.ImageUrl = "";
                        //}





                    }
                    if (j == 1)
					{
						TableCell infoCell = new TableCell();

						Label lb = new Label();
						DateTime photoDate;
						if (DateTime.TryParse(dt.Rows[i]["photo_date"].ToString(), out photoDate))
						{
							lb.Text = "撮影日時：" + photoDate.ToString("yyyy/MM/dd HH:mm:ss");
						}
						else
						{
							lb.Text = "撮影日時：";
						}


						infoCell.Controls.Add(lb);
						infoRow[j].Controls.Add(infoCell);
					}





                    if (j == 2)
					{
						TableCell infoCell = new TableCell();

						HiddenField hdn2 = new HiddenField();
						hdn2.ID = "hdnTitle_" + i.ToString();
						hdn2.Value = dt.Rows[i]["title"].ToString();
						infoCell.Controls.Add(hdn2);
						HiddenField hdn3 = new HiddenField();
						hdn3.ID = "hdnImagePath_" + i.ToString();

						if (string.IsNullOrEmpty(dt.Rows[i]["image_path"].ToString()))
						{
							hdn3.Value = "";
						}
						else
						{
							//Blobに保存
							CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
							CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
							CloudBlobContainer container = blobClient.GetContainerReference(getConst("container"));
							CloudBlockBlob blockBlob = container.GetBlockBlobReference(dt.Rows[i]["image_path"].ToString());

							if (blobExist(blockBlob))
							{
								hdn3.Value = getConst("endPoint") + getConst("container") + "/" + dt.Rows[i]["image_path"].ToString();
							}
							else
							{
								hdn3.Value = "";
							}
						}

						//hdn3.Value = dt.Rows[i]["image_path"].ToString();
						infoCell.Controls.Add(hdn3);

						Label lb = new Label();
						lb.Text = "拡大表示";
						lb.Attributes["onclick"] = "fncImage(this)";
						lb.CssClass = "btn btnSizeM";
						infoCell.Controls.Add(lb);
						infoRow[j].Controls.Add(infoCell);


						TableCell infoCell2 = new TableCell();

						HiddenField hdn = new HiddenField();
						hdn.ID = "hdnImageNo_" + i.ToString();
						hdn.Value = dt.Rows[i]["image_no"].ToString();
						infoCell2.Controls.Add(hdn);
						HiddenField hdn4 = new HiddenField();
						hdn4.ID = "hdnImagePath2_" + i.ToString();
						hdn4.Value = hdn3.Value;
						infoCell2.Controls.Add(hdn4);

						if (hdnExclusive.Value != "2")
						{
							Label lb2 = new Label();
							lb2.Text = "削除";
							lb2.Attributes["onclick"] = "fncDel(this)";
							lb2.CssClass = "btn btnSizeM";
							infoCell2.Controls.Add(lb2);
							infoRow[j].Controls.Add(infoCell2);
						}
						infoRow[j].CssClass = "btmBorder";
					}
					tbl1.Rows.Add(infoRow[j]);
				}
				panelImage.Controls.Add(tbl1);
			}

			js("setRestsTitle(" + cnt +")");

			return 0;
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "画像リスト作成エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return -1;
		}
	}

	/// <summary>
	/// 画像参照
	/// </summary>
	/// <returns></returns>
	public string getImageInfo() {
		try
		{
			string tbl = "";
			//Dim dateTmp As String = ""

			if (ViewState["dt0"] == null ) { return ""; };

			DataTable dt = (DataTable)ViewState["dt0"];
			int cnt = dt.Rows.Count;

			for (int i = 0; i < cnt; i++ ) {


				tbl += "<div>";
				tbl += "<input type='hidden' value='" + dt.Rows[i]["image_no"].ToString() + "'>";
				tbl += "<input type='hidden' value='" + dt.Rows[i]["title"].ToString() + "'>";
				tbl += "<input type='hidden' value='" + dt.Rows[i]["image_path"].ToString() + "'>";
				tbl += "<img class='imageList' alt='' src='" + dt.Rows[i]["image_path"].ToString() + "' onclick='fncImage(this)')>";
				tbl += "<div>" + dt.Rows[i]["photo_date"].ToString() + "</div>";
				tbl += "<div >タイトル：" + dt.Rows[i]["title"].ToString() +  "</div>";
				tbl += "";
                //tbl += "<div onclick='callTouchKey()'>ペン</div>";
                tbl += "<div onclick='fncImage(this)'>拡大表示</div>";
				tbl += "<div onclick='fncDel(this)'>削除</div>";
				tbl += "</div>";
			}

			return tbl;
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "画像リスト作成エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return "";
		}

	}

	/// <summary>
	/// 削除
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void btnDel_Click(object sender, EventArgs e)
	{
		try
		{
			logLong(3, businessName, displayName, "削除ボタン押下", hdnPatientId.Value, hdnPatientName.Value, "");

			string userId = "";
			rtn = exclusiveCheck("firstVisit", hdnPatientId.Value, "", ref userId);
			if (rtn == 0)
			{
				if (userId != getSession("LOGIN_ID") && hdnExclusive.Value == "1")
				{
					//自分が排他していないので、編集権を取られている。
					js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の削除ができませんでした。<br />編集中に別の端末から編集作業されました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"0\",\"\")\');");
					return;
				}
				else
				{
					//自分が排他してるので保存可能
				}
			}
			else
			{
				js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の削除に失敗しました。<br />管理者に連絡してください。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', 'alertErr()');");
				return;
			}

			rtn = del();
			if (rtn == 0)
			{
				js("alertmsg('情報', '<article>選択した画像を削除しました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"9\",\"\")\');");
			}
			else
			{
				logLong(4, businessName, displayName, "削除処理エラー", hdnPatientId.Value, hdnPatientName.Value, "");
				js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>画像の削除に失敗しました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"9\",\"\")\');");
			}

			//js("fncClose();");
			panelImage.Controls.Clear();
			imageSet();
			createTable();

		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "削除ボタン押下エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
		}
	}
	private int del()
	{
		try
		{
			//選択されている患者ID。
			string patientId = hdnPatientId.Value;
			int imageNo = int.Parse(hdnImageNo.Value);

			string sqlString = "";

			//削除
			sqlString = "update tbl_patient_image set mask = '1' ";
			sqlString += "where hosp_id = '" + getSession("HOSP_ID") + "'";
			sqlString += "  and patient_id = '" + patientId + "'";
			sqlString += "  and image_no = " + imageNo;

			sqlrtn = sqlUpdate(sqlString);
			if (!sqlrtn)
			{
				logLong(4, businessName, displayName, "削除エラー" + sqlString, hdnPatientId.Value, hdnPatientName.Value, "");
				return -1;
			}




			//Blobから削除
			string filePath = "";

			//Pathセット
			sqlString = "select image_path from tbl_patient_image ";
			sqlString += "where hosp_id = '" + getSession("HOSP_ID") + "'";
			sqlString += "  and patient_id = '" + patientId + "'";
			sqlString += "  and image_no = " + imageNo;

			filePath = sqlSelect(sqlString, "image_path");

			if (!string.IsNullOrEmpty(filePath))
			{
				CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
				CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
				CloudBlobContainer container = blobClient.GetContainerReference(getConst("container"));
				CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);
				blockBlob.DeleteAsync();
			}



			return 0;
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "削除エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return -1;
		}

	}

	/// <summary>
	/// 更新
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void btnUpdate_Click(object sender, EventArgs e)
	{
		try
		{
			logLong(3, businessName, displayName, "更新ボタン押下", hdnPatientId.Value, hdnPatientName.Value, "");

			string userId = "";
			rtn = exclusiveCheck("firstVisit", hdnPatientId.Value, "", ref userId);
			if (rtn == 0)
			{
				if (userId != getSession("LOGIN_ID") && hdnExclusive.Value == "1")
				{
					//自分が排他していないので、編集権を取られている。
					js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>タイトルの更新ができませんでした。<br />編集中に別の端末から編集作業されました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\", 'ＯＫ', \'alertClose(\"0\",\"\")\');");
					return;
				}
				else
				{
					//自分が排他してるので保存可能
				}
			}
			else
			{
				js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>タイトルの更新に失敗しました。<br />管理者に連絡してください。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', 'alertErr()');");
				return;
			}

			rtn = update();
			if (rtn == 0)
			{
				js("alertmsg('情報', '<article>タイトルを変更しました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"9\",\"\")\');");
			}
			else
			{
				logLong(4, businessName, displayName, "更新処理エラー", hdnPatientId.Value, hdnPatientName.Value, "");
				js("setSvgTitleIconColor(1);alertmsg('エラー', '<article>タイトルの更新に失敗しました。</article>',\"<use xmlns:xlink=\'http://www.w3.org/1999/xlink\' xlink:href=" + ResolveUrl("~/Img/icons_system.svg#icon-person") + "></use>\",'ＯＫ', \'alertClose(\"9\",\"\")\');");
			}

			//js("fncClose();");
			panelImage.Controls.Clear();
			imageSet();
			createTable();

		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "更新ボタン押下エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
		}
	}
	private int update()
	{
		try
		{
			List<SqlParameter> lstwhere = new List<SqlParameter>();
			//選択されている患者ID。
			string patientId = hdnPatientId.Value;
			int imageNo = int.Parse(hdnImageNo.Value);

			string sqlString = "";
			//削除
			sqlString = "update tbl_patient_image set title = @TITLE ";
			sqlString += "where hosp_id = '" + getSession("HOSP_ID") + "'";
			sqlString += "  and patient_id = '" + patientId + "'";
			sqlString += "  and image_no = " + imageNo;

			lstwhere.Add(createSqlParameter("@TITLE", SqlDbType.VarChar, hdnTitle.Value));

			SqlParameter[] param = lstwhere.ToArray();
			sqlrtn = sqlUpdate(sqlString, param);
			if (!sqlrtn)
			{
				logLong(4, businessName, displayName, "更新エラー" + sqlString, hdnPatientId.Value, hdnPatientName.Value, "");
				return -1;
			}
			return 0;
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "更新エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			return -1;
		}

	}


	/// <summary>
	/// ファイルのリセット
	/// </summary>
	protected void btnReset_Click(object sender, EventArgs e)
	{
		try
		{
			if (string.IsNullOrEmpty(hdnFilePath.Value))
			{
				imgFile.ImageUrl = "";
			}
			else
			{
				//Blobに保存
				CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
				CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
				CloudBlobContainer container = blobClient.GetContainerReference(getConst("container"));
				CloudBlockBlob blockBlob = container.GetBlockBlobReference(hdnFilePath.Value);

				if (blobExist(blockBlob))
				{
					imgFile.ImageUrl = getConst("endPoint") + getConst("container") + "/" + hdnFilePath.Value;
				}
				else
				{
					imgFile.ImageUrl = "";
				}
			}
		}
		catch (Exception ex)
		{
			logLong(4, businessName, displayName, "画像表示エラー:" + ex.Message, hdnPatientId.Value, hdnPatientName.Value, "");
			imgFile.ImageUrl = "";
			return ;
		}
	}


	private bool blobExist(CloudBlockBlob blob)
	{
		try
		{
			blob.FetchAttributes();
			return true;
		}
		catch (Exception ex)
		{
			return false;
		}
	}
}


