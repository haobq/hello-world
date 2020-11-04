using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

/// <summary>
/// 共通関数
/// </summary>
public partial class App_Function : System.Web.UI.Page
{

    public const string cmnName = "共通関数";

    public App_Function()
    {
        this.setSess(new SessImpl(this, new SessionIdImpl()));
    }

    /// <summary>
    /// web.configの値を呼ぶ
    /// </summary>
    /// <param name="str">ID</param>
    /// <returns>nullの場合は空文字を返す</returns>
    public string getConst(string str)
    {
        var conStr = ConfigurationManager.AppSettings[str];
        return (conStr != null) ? conStr : "";
    }

    /// <summary>
    /// セッション変数内文字列取得
    /// </summary>
    /// <param name="str">セッションID</param>
    /// <returns>nullの場合は空文字を返す</returns>
    public string getSession(string str)
    {
        return (Session[str] != null) ? Session[str].ToString() : "";
    }

    /// <summary>
    /// ViewState変数内文字列取得
    /// </summary>
    /// <param name="str">セッションID</param>
    /// <returns>nullの場合は空文字を返す</returns>
    public string getViewState(string str)
    {
        return (ViewState[str] != null) ? ViewState[str].ToString() : "";
    }

    /// <summary>
    /// クエリーストリング取得
    /// </summary>
    /// <param name="str">keys</param>
    /// <returns>nullの場合は空文字を返す</returns>
    /// calendar.aspx?id=001&flg=1&keys=values
    /// 半角英数字以外を使うときはUrlEncodeしてから渡してください
    public string getQueryString(string str)
    {
        return (Request.QueryString[str] != null) ? System.Web.HttpUtility.UrlDecode(Request.QueryString[str]) : "";
    }

    public DateTime getLastDayMonth(DateTime d)
    {
        d = d.AddMonths(1);
        d = new DateTime(d.Year, d.Month, 1);
        d = d.AddDays(-1.0);
        return d;
    }

    #region DB関連
    SqlConnection dbCon = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString);
    SqlDataAdapter dbAda = new SqlDataAdapter();
    SqlCommand dbCom = new SqlCommand();
    Object thisLock = new Object();

    List<String> Hosplists = sqlselectTable();

    /// <summary>
    /// sqlセレクトアイテム
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="selectItem"></param>
    /// <returns></returns>
    public string sqlSelect(string sentence, string selectItem)
    {
        var returnItem = "";
        lock (thisLock)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbAda.SelectCommand = dbCom;
                dbCom.CommandText = sentence;
                dbAda.Fill(dataTable);

                if (dataTable.Rows.Count <= 0) returnItem = "";
                else returnItem = dataTable.Rows[0][selectItem].ToString();

                dataTable.Dispose();
            }
            catch (SqlException ex)
            {
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                returnItem = "";
            }
            finally
            {
                dbCon.Close();
            }
        }
        return returnItem;
    }

    /// <summary>
    /// SQLインサート
    /// </summary>
    /// <param name="p">インサート文（第二引数に何か入れるとエラーログを呼ばない）</param>
    /// <returns></returns>
    public bool sqlInsert(params string[] p)
    {
        var rtn = true;
        lock (thisLock)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbCom.CommandText = p[0];
                dbTra = dbCon.BeginTransaction();
                dbCom.Transaction = dbTra;
                dbCom.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                if (p.Length == 1)
                {
                    dbCon.Close();
                    logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                    logError(p[0]);
                }
                else
                {
                    jsl(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                    jsl(p[0]);
                }
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLアップデート
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public bool sqlUpdate(string sentence)
    {
        var rtn = true;
        lock (thisLock)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbCom.CommandText = sentence;
                dbTra = dbCon.BeginTransaction();
                dbCom.Transaction = dbTra;
                dbCom.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLデリート
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public bool sqlDelete(string sentence)
    {
        var rtn = true;
        lock (thisLock)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbCom.CommandText = sentence;
                dbTra = dbCon.BeginTransaction();
                dbCom.Transaction = dbTra;
                dbCom.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// sqlセレクトアイテム
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="selectItem"></param>
    /// <returns></returns>
    public string sqlSelect(string sentence, string selectItem, SqlParameter[] param)
    {
        var returnItem = "";
        lock (thisLock)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbAda.SelectCommand = dbCom;
                dbCom.CommandText = sentence;
                dbCom.Parameters.Clear();
                dbCom.Parameters.AddRange(param);
                dbAda.Fill(dataTable);

                if (dataTable.Rows.Count <= 0) returnItem = "";
                else returnItem = dataTable.Rows[0][selectItem].ToString();

                dataTable.Dispose();
            }
            catch (SqlException ex)
            {
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                returnItem = "";
            }
            finally
            {
                dbCon.Close();
            }
        }
        return returnItem;
    }

    /// <summary>
    /// SQLアップデート
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public bool sqlUpdate(string sentence, SqlParameter[] param)
    {
        var rtn = true;
        lock (thisLock)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbCom.CommandText = sentence;
                dbCom.Parameters.Clear();
                dbCom.Parameters.AddRange(param);
                dbTra = dbCon.BeginTransaction();
                dbCom.Transaction = dbTra;
                dbCom.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }


    /// <summary>
    /// SQLデリート
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public bool sqlDelete(string sentence, SqlParameter[] param)
    {
        var rtn = true;
        lock (thisLock)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbCom.CommandText = sentence;
                dbCom.Parameters.Clear();
                dbCom.Parameters.AddRange(param);
                dbTra = dbCon.BeginTransaction();
                dbCom.Transaction = dbTra;
                dbCom.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLトランザクション
    /// </summary>
    /// <param name="p">SQL文</param>
    /// <returns></returns>
    public bool sqlTran(params string[] p)
    {
        var rtn = true;
        lock (thisLock)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbAda.SelectCommand = dbCom;
                dbAda.InsertCommand = dbCom;
                dbAda.UpdateCommand = dbCom;
                dbAda.DeleteCommand = dbCom;

                SqlTransaction dbTra = null;
                dbTra = dbCon.BeginTransaction();
                dbCom.Transaction = dbTra;

                for (var i = 0; i < p.Length; i++)
                {
                    try
                    {
                        dbCom.CommandText = p[i];
                        dbCom.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        dbTra.Rollback();
                        dbCon.Close();
                        logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                        logError(p[i]);
                        throw ex;
                    }
                }
                dbTra.Commit();
            }
            catch (SqlException ex)
            {
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLトランザクション
    /// </summary>
    /// <param name="p">SQL文</param>
    /// <returns></returns>
    public bool sqlTran(string[] p, List<SqlParameter[]> lstparam)
    {
        var rtn = true;
        lock (thisLock)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbAda.SelectCommand = dbCom;
                dbAda.InsertCommand = dbCom;
                dbAda.UpdateCommand = dbCom;
                dbAda.DeleteCommand = dbCom;

                SqlTransaction dbTra = null;
                dbTra = dbCon.BeginTransaction();
                dbCom.Transaction = dbTra;

                for (var i = 0; i < p.Length; i++)
                {
                    try
                    {
                        dbCom.CommandText = p[i];
                        dbCom.Parameters.Clear();
                        if (lstparam[i] != null) { dbCom.Parameters.AddRange(lstparam[i]); }
                        dbCom.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        dbTra.Rollback();
                        dbCon.Close();
                        logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                        logError(p[i]);
                        throw ex;
                    }
                }
                dbTra.Commit();
            }
            catch (SqlException ex)
            {
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLセレクトテーブル
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="dataTbl"></param>
    /// <returns></returns>
    public bool sqlSelectTable(string sentence, ref DataTable dataTbl)
    {
        var rtn = true;
        lock (thisLock)
        {
            try
            {
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbAda.SelectCommand = dbCom;
                dbCom.CommandText = sentence;
                dbAda.Fill(dataTbl);
                dbCon.Close();
            }
            catch (SqlException ex)
            {
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }
    /// <summary>
    /// SQLセレクトテーブル
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="dataTbl"></param>
    /// <returns></returns>
    public bool sqlSelectTable(string sentence, SqlParameter[] param, ref DataTable dataTbl)
    {
        var rtn = true;
        lock (thisLock)
        {
            try
            {
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbAda.SelectCommand = dbCom;
                dbCom.CommandText = sentence;
                dbCom.Parameters.Clear();
                dbAda.SelectCommand.Parameters.AddRange(param);
                dbAda.Fill(dataTbl);
                dbCon.Close();
            }
            catch (SqlException ex)
            {
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }

    /*選択された製品にあわせて、Dr/DH/その他のリストの内容を切り替える。 --- */
    /// <summary>
    /// SQLセレクトテーブル
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="dataTbl"></param>
    /// <returns></returns>
    public bool sqlSelectTable(string hosp_id,string sentence, SqlParameter[] param, ref DataTable dataTbl)
    {
        var rtn = true;

        if (!Hosplists.Contains(hosp_id))
        {
            return false;
        }
        
        var replace = ConfigurationManager.ConnectionStrings["sqlsmh"].ConnectionString;
        SqlConnection dbSmh = new SqlConnection(replace.Replace("HospID", hosp_id));

        lock (thisLock)
        {
            try
            {
                dbSmh.Open();
                dbCom.Connection = dbSmh;
                dbAda.SelectCommand = dbCom;
                dbCom.CommandText = sentence;
                dbCom.Parameters.Clear();
                dbAda.SelectCommand.Parameters.AddRange(param);
                dbAda.Fill(dataTbl);
                dbCon.Close();
            }
            catch (SqlException ex)
            {
                dbSmh.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
              dbSmh.Close();
            }
        }
        return rtn;
    }

  ///summary>
  ///医院リストを取得
  ///</summary>
  /// <param name="sentence"></param>
  /// <param name="dataTbl"></param>
  /// <returns></returns>
  public static List<string> sqlselectTable()
  {
    SqlConnection dbMas = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlmaster"].ConnectionString);
    List<string> list = new List<string>();
    DataTable dataTbl = new DataTable(); 
    SqlCommand dbCom1 = new SqlCommand();
    SqlDataAdapter dbAda1 = new SqlDataAdapter();
    try
      {
        dbMas.Open();
        dbCom1.Connection = dbMas;
        dbAda1.SelectCommand = dbCom1;
        dbCom1.CommandText = "select name from sys.databases";
        dbAda1.Fill(dataTbl);

        for (int i = 0; i < dataTbl.Rows.Count; i++)
        {
           String name = dataTbl.Rows[i]["name"].ToString();

          if (name.StartsWith("ECYO2_"))
          {
            list.Add(name.Split('_')[1]);
          }
        }

        dbMas.Close();
      }
      catch (SqlException ex)
      {
      dbMas.Close();
      }
      finally
      {
      dbMas.Close();
      }
    
    return list;
  }

  /// <summary>
  /// SQLパラメーターを作成する
  /// </summary>
  /// <param name="parameterName"></param>
  /// <param name="dbType"></param>
  /// <param name="value"></param>
  /// <returns></returns>
  public SqlParameter createSqlParameter(string parameterName, SqlDbType dbType, object value)
    {
        var rtn = new SqlParameter();
        rtn.ParameterName = parameterName;
        rtn.SqlDbType = dbType;
        rtn.Direction = ParameterDirection.Input;
        rtn.Value = value;
        return rtn;
    }

    /// <summary>
    /// SQLセレクトデータセット
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="dataSet"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public bool sqlSelectDataSet(string sentence, ref System.Data.DataSet dataSet, string tableName)
    {
        var rtn = true;
        lock (thisLock)
        {
            try
            {
                dbCon.Open();
                dbCom.Connection = dbCon;
                dbAda.SelectCommand = dbCom;
                dbCom.CommandText = sentence;
                dbAda.Fill(dataSet, tableName);
            }
            catch (SqlException ex)
            {
                dbCon.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbCon.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLエスケープ
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string sqlTabooChar(string str)
    {
        return str.Replace("'", "''");
    }

    //Public Function OracleTabooChar(ByVal value As String) As String

    #endregion

    #region 状態把握ナビ・DB関連
    SqlConnection dbConNavi = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlsvrNavi"].ConnectionString);
    SqlDataAdapter dbAdaNavi = new SqlDataAdapter();
    SqlCommand dbComNavi = new SqlCommand();
    Object thisLockNavi = new Object();

    /// <summary>
    /// sqlセレクトアイテム
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="selectItem"></param>
    /// <returns></returns>
    public string sqlSelectNavi(string sentence, string selectItem)
    {
        var returnItem = "";
        lock (thisLockNavi)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbAdaNavi.SelectCommand = dbComNavi;
                dbComNavi.CommandText = sentence;
                dbAdaNavi.Fill(dataTable);

                if (dataTable.Rows.Count <= 0) returnItem = "";
                else returnItem = dataTable.Rows[0][selectItem].ToString();

                dataTable.Dispose();
            }
            catch (SqlException ex)
            {
                dbConNavi.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                returnItem = "";
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return returnItem;
    }

    /// <summary>
    /// SQLインサート
    /// </summary>
    /// <param name="p">インサート文（第二引数に何か入れるとエラーログを呼ばない）</param>
    /// <returns></returns>
    public bool sqlInsertNavi(params string[] p)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbComNavi.CommandText = p[0];
                dbTra = dbConNavi.BeginTransaction();
                dbComNavi.Transaction = dbTra;
                dbComNavi.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                if (p.Length == 1)
                {
                    dbConNavi.Close();
                    logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                    logError(p[0]);
                }
                else
                {
                    jsl(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                    jsl(p[0]);
                }
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLアップデート
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public bool sqlUpdateNavi(string sentence)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbComNavi.CommandText = sentence;
                dbTra = dbConNavi.BeginTransaction();
                dbComNavi.Transaction = dbTra;
                dbComNavi.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                dbConNavi.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLデリート
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public bool sqlDeleteNavi(string sentence)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbComNavi.CommandText = sentence;
                dbTra = dbConNavi.BeginTransaction();
                dbComNavi.Transaction = dbTra;
                dbComNavi.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                dbConNavi.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// sqlセレクトアイテム
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="selectItem"></param>
    /// <returns></returns>
    public string sqlSelectNavi(string sentence, string selectItem, SqlParameter[] param)
      {
          var returnItem = "";
          lock (thisLockNavi)
          {
              try
              {
                  DataTable dataTable = new DataTable();
                  dbConNavi.Open();
                  dbComNavi.Connection = dbConNavi;
                  dbAdaNavi.SelectCommand = dbComNavi;
                  dbComNavi.CommandText = sentence;
                  dbComNavi.Parameters.Clear();
                  dbComNavi.Parameters.AddRange(param);
                  dbAdaNavi.Fill(dataTable);

                  if (dataTable.Rows.Count <= 0) returnItem = "";
                  else returnItem = dataTable.Rows[0][selectItem].ToString();

                  dataTable.Dispose();
              }
              catch (SqlException ex)
              {
                  dbConNavi.Close();
                  logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                  logError(sentence);
                  returnItem = "";
              }
              finally
              {
                  dbConNavi.Close();
              }
          }
          return returnItem;
      }

    /// <summary>
    /// SQLアップデート
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public bool sqlUpdateNavi(string sentence, SqlParameter[] param)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbComNavi.CommandText = sentence;
                dbComNavi.Parameters.Clear();
                dbComNavi.Parameters.AddRange(param);
                dbTra = dbConNavi.BeginTransaction();
                dbComNavi.Transaction = dbTra;
                dbComNavi.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                dbConNavi.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }


    /// <summary>
    /// SQLデリート
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public bool sqlDeleteNavi(string sentence, SqlParameter[] param)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            SqlTransaction dbTra = null;
            try
            {
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbComNavi.CommandText = sentence;
                dbComNavi.Parameters.Clear();
                dbComNavi.Parameters.AddRange(param);
                dbTra = dbConNavi.BeginTransaction();
                dbComNavi.Transaction = dbTra;
                dbComNavi.ExecuteNonQuery();
                dbTra.Commit();
            }
            catch (Exception ex)
            {
                dbTra.Rollback();
                dbConNavi.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLトランザクション
    /// </summary>
    /// <param name="p">SQL文</param>
    /// <returns></returns>
    public bool sqlTranNavi(params string[] p)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbAdaNavi.SelectCommand = dbComNavi;
                dbAdaNavi.InsertCommand = dbComNavi;
                dbAdaNavi.UpdateCommand = dbComNavi;
                dbAdaNavi.DeleteCommand = dbComNavi;

                SqlTransaction dbTra = null;
                dbTra = dbConNavi.BeginTransaction();
                dbComNavi.Transaction = dbTra;

                for (var i = 0; i < p.Length; i++)
                {
                    try
                    {
                        dbComNavi.CommandText = p[i];
                        dbComNavi.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        dbTra.Rollback();
                        dbConNavi.Close();
                        logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                        logError(p[i]);
                        throw ex;
                    }
                }
                dbTra.Commit();
            }
            catch (SqlException ex)
            {
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLトランザクション
    /// </summary>
    /// <param name="p">SQL文</param>
    /// <returns></returns>
    public bool sqlTranNavi(string[] p, List<SqlParameter[]> lstparam)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbAdaNavi.SelectCommand = dbComNavi;
                dbAdaNavi.InsertCommand = dbComNavi;
                dbAdaNavi.UpdateCommand = dbComNavi;
                dbAdaNavi.DeleteCommand = dbComNavi;

                SqlTransaction dbTra = null;
                dbTra = dbConNavi.BeginTransaction();
                dbComNavi.Transaction = dbTra;

                for (var i = 0; i < p.Length; i++)
                {
                    try
                    {
                        dbComNavi.CommandText = p[i];
                        dbComNavi.Parameters.Clear();
                        if (lstparam[i] != null) { dbComNavi.Parameters.AddRange(lstparam[i]); }
                        dbComNavi.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        dbTra.Rollback();
                        dbConNavi.Close();
                        logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                        logError(p[i]);
                        throw ex;
                    }
                }
                dbTra.Commit();
            }
            catch (SqlException ex)
            {
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLセレクトテーブル
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="dataTbl"></param>
    /// <returns></returns>
    public bool sqlSelectTableNavi(string sentence, ref DataTable dataTbl)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            try
            {
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbAdaNavi.SelectCommand = dbComNavi;
                dbComNavi.CommandText = sentence;
                dbAdaNavi.Fill(dataTbl);
                dbConNavi.Close();
            }
            catch (SqlException ex)
            {
                dbConNavi.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }
    /// <summary>
    /// SQLセレクトテーブル
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="dataTbl"></param>
    /// <returns></returns>
    public bool sqlSelectTableNavi(string sentence, SqlParameter[] param, ref DataTable dataTbl)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            try
            {
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbAdaNavi.SelectCommand = dbComNavi;
                dbComNavi.CommandText = sentence;
                dbComNavi.Parameters.Clear();
                dbAdaNavi.SelectCommand.Parameters.AddRange(param);
                dbAdaNavi.Fill(dataTbl);
                dbConNavi.Close();
            }
            catch (SqlException ex)
            {
                dbConNavi.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLセレクトデータセット
    /// </summary>
    /// <param name="sentence"></param>
    /// <param name="dataSet"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public bool sqlSelectDataSetNavi(string sentence, ref System.Data.DataSet dataSet, string tableName)
    {
        var rtn = true;
        lock (thisLockNavi)
        {
            try
            {
                dbConNavi.Open();
                dbComNavi.Connection = dbConNavi;
                dbAdaNavi.SelectCommand = dbComNavi;
                dbComNavi.CommandText = sentence;
                dbAdaNavi.Fill(dataSet, tableName);
            }
            catch (SqlException ex)
            {
                dbConNavi.Close();
                logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                logError(sentence);
                rtn = false;
            }
            finally
            {
                dbConNavi.Close();
            }
        }
        return rtn;
    }

    /// <summary>
    /// SQLトランザクション
    /// </summary>
    /// <param name="p">SQL文</param>
    /// <returns></returns>
    public bool sqlTranWithNavi(string[] pWith, List<SqlParameter[]> lstparamWith, string[] pNavi, List<SqlParameter[]> lstparamNavi)
    {
        var rtn = true;
        lock (thisLock)
        {
            lock (thisLockNavi)
            {
                try
                {
                    DataTable dataTable = new DataTable();

                    dbCon.Open();
                    dbCom.Connection = dbCon;
                    dbAda.SelectCommand = dbCom;
                    dbAda.InsertCommand = dbCom;
                    dbAda.UpdateCommand = dbCom;
                    dbAda.DeleteCommand = dbCom;

                    SqlTransaction dbTraWith = null;
                    dbTraWith = dbCon.BeginTransaction();
                    dbCom.Transaction = dbTraWith;

                    dbConNavi.Open();
                    dbComNavi.Connection = dbConNavi;
                    dbAdaNavi.SelectCommand = dbComNavi;
                    dbAdaNavi.InsertCommand = dbComNavi;
                    dbAdaNavi.UpdateCommand = dbComNavi;
                    dbAdaNavi.DeleteCommand = dbComNavi;

                    SqlTransaction dbTraNavi = null;
                    dbTraNavi = dbConNavi.BeginTransaction();
                    dbComNavi.Transaction = dbTraNavi;

                    for (var i = 0; i < pWith.Length; i++)
                    {
                        try
                        {
                            dbCom.CommandText = pWith[i];
                            dbCom.Parameters.Clear();
                            if (lstparamWith[i] != null) { dbCom.Parameters.AddRange(lstparamWith[i]); }
                            dbCom.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            dbTraWith.Rollback();
                            dbTraNavi.Rollback();
                            dbCon.Close();
                            dbConNavi.Close();
                            logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                            logError(pWith[i]);
                            throw ex;
                        }
                    }

                    for (var i = 0; i < pNavi.Length; i++)
                    {
                        try
                        {
                            dbComNavi.CommandText = pNavi[i];
                            dbComNavi.Parameters.Clear();
                            if (lstparamNavi[i] != null) { dbComNavi.Parameters.AddRange(lstparamNavi[i]); }
                            dbComNavi.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            dbTraWith.Rollback();
                            dbTraNavi.Rollback();
                            dbCon.Close();
                            dbConNavi.Close();
                            logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                            logError(pNavi[i]);
                            throw ex;
                        }
                    }
                    dbTraWith.Commit();
                    dbTraNavi.Commit();
                }
                catch (SqlException ex)
                {
                    logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
                    rtn = false;
                }
                finally
                {
                    dbCon.Close();
                    dbConNavi.Close();
                }
            }
        }
        
        return rtn;
    }

    #endregion

    #region ログ
    //public bool logLong(short hospId, short kind, string businessName, string displayName,
    //  string description, string patientId, string patientName, string exmDate)
    public bool logLong(short kind, string businessName, string displayName,
      string description, string patientId, string patientName, string exmDate)
    {
        try
        {
            DateTime dTime = new DateTime();
            string exmDateTmp = (DateTime.TryParse(exmDate, out dTime)) ? dTime.ToString("yyyy/MM/dd") : "1999/01/01";

            var sql = new StringBuilder();
            sql.AppendLine("insert tbl_operation_log values(");
            sql.AppendLine("@HOSP_ID,"); //HOSP_ID:varchar(10)
            sql.AppendLine("@KIND,"); //KIND:smallint
            sql.AppendLine("getdate(),"); //DO_DATE:datetime
            sql.AppendLine("@BUSINESS_NAME,"); //BUSINESS_NAME:varchar(40)
            sql.AppendLine("@DISPLAY_NAME,"); //DISPLAY_NAME:varchar(40)
            sql.AppendLine("@DESCRIPTION,");//DESCRIPTION:varchar(1000)
            sql.AppendLine("@PATIENT_ID,"); //PATIENT_ID:varchar(10)
            sql.AppendLine("@PATIENT_NAME,"); //PATIENT_NAME:varchar(40)
            sql.AppendLine("CONVERT(datetime, @EXM_DATE),"); //EXM_DATE:date
            sql.AppendLine("@TERMINAL_ID,"); //TERMINAL_ID:varchar(20)
            sql.AppendLine("@LOGIN_ID,"); //LOGIN_ID:varchar(20)
            sql.AppendLine("@TEAM_ID,"); //TEAM_ID:varchar(10)
            sql.AppendLine("@GROUP_ID"); //GROUP_ID:varchar(20)
            sql.AppendLine(")");

            var param = new SqlParameter[] {
        createSqlParameter("@HOSP_ID", SqlDbType.VarChar, omitLeftB(sqlTabooChar(mySession.hosp_id), 10)), //HOSP_ID:varchar(10)
        createSqlParameter("@KIND", SqlDbType.SmallInt, kind), //KIND:smallint
        createSqlParameter("@BUSINESS_NAME", SqlDbType.VarChar, omitLeftB(sqlTabooChar(businessName), 40)), //BUSINESS_NAME:varchar(40)
        createSqlParameter("@DISPLAY_NAME", SqlDbType.VarChar, omitLeftB(sqlTabooChar(displayName), 40)), //DISPLAY_NAME:varchar(40)
        createSqlParameter("@DESCRIPTION", SqlDbType.VarChar, omitLeftB(sqlTabooChar(description), 1000)),//DESCRIPTION:varchar(1000)
        createSqlParameter("@PATIENT_ID", SqlDbType.VarChar, omitLeftB(sqlTabooChar(patientId), 10)), //PATIENT_ID:varchar(10)
        createSqlParameter("@PATIENT_NAME", SqlDbType.VarChar, omitLeftB(sqlTabooChar(patientName), 40)), //PATIENT_NAME:varchar(40)
        createSqlParameter("@EXM_DATE", SqlDbType.VarChar, exmDateTmp), //EXM_DATE:date
        createSqlParameter("@TERMINAL_ID", SqlDbType.VarChar, omitLeftB(sqlTabooChar(mySession.terminal_id), 20)), //TERMINAL_ID:varchar(20)
        createSqlParameter("@LOGIN_ID", SqlDbType.VarChar, omitLeftB(sqlTabooChar(mySession.login_id), 20)), //LOGIN_ID:varchar(20)
        createSqlParameter("@TEAM_ID", SqlDbType.VarChar, omitLeftB(sqlTabooChar(mySession.team_id), 10)), //TEAM_ID:varchar(10)
        createSqlParameter("@GROUP_ID", SqlDbType.VarChar, omitLeftB(sqlTabooChar(mySession.group_id), 20)), //GROUP_ID:varchar(20)
      };

            var p = new string[] { sql.ToString() };
            var lstparam = new SqlParameter[][] { param }.ToList();
            sqlTran(p, lstparam);
        }

        catch (Exception ex)
        {
            jsl(ex.ToString());
            return false;
        }
        return true;
    }

    public bool logError(string description)
    {
        //logLong(-1, -1, "", "", description, "", "", "");
        logLong(4, "", "", description, "", "", "");
        return true;
    }

    #endregion





    /// <summary>
    /// ランダムな英数字を生成する。前回の値と別のものを作りたい場合は、第二引数に文字列を渡せば違うものを生成する。
    /// </summary>
    /// <param name="cnt">生成する文字数（255文字まで）</param>
    /// <param name="p">前回の値</param>
    /// <returns>ランダムな英数字</returns>
    public string getKey(byte cnt, params string[] p)
    {
        string tmp = "", pre = "", pw = "";
        if (p.Length > 0) pre = p[0];
        do
        {
            do
            {
                //128文字が上限なため
                pw = System.Web.Security.Membership.GeneratePassword(128, 0);
                //記号とアンダースコアの置換
                pw = System.Text.RegularExpressions.Regex.Replace(pw, "\\W", "").Replace("_", "");
                tmp += pw;
            } while (cnt >= tmp.Length);
            tmp = tmp.Substring(0, cnt);
        } while (tmp == pre);
        return tmp;
    }

    /// <summary>
    /// バージョンを取得する
    /// </summary>
    /// <returns>バージョン</returns>
    /// <remarks>キャッシュ用。リリース時はバージョンを固定にする</remarks>
    public string getVersion()
    {
        string rtn = "";
        //リリース時に更新日にする
        //rtn = 20140701
        rtn = getKey(8);
        return rtn;
    }

    #region JavaScript
    /// <summary>
    /// JavaScript実行
    /// </summary>
    /// <param name="p">実行させたいJavaScript</param>
    public void js(string p)
    {
        if (p.Substring(p.Length - 1, 1) != ";") p += ";";
        Page cp = (Page)HttpContext.Current.CurrentHandler;
        ScriptManager.RegisterStartupScript(cp, Page.GetType(), getKey(16), p, true);
    }

    /// <summary>
    /// JavaScriptをsetTimeoutと実行する
    /// </summary>
    /// <param name="p">実行させたいJavaScript</param>
    /// <param name="ms">ミリ秒。省略すると0</param>
    public void jst(string p, params int[] ms)
    {
        int t = (ms.Length == 0) ? 0 : ms[0];
        if (p.Substring(p.Length - 1, 1) != ";") p += ";";
        string tmp = string.Format("setTimeout(function(){0}{1}{2},{3});", "{", p, "}", t);
        Page cp = (Page)HttpContext.Current.CurrentHandler;
        ScriptManager.RegisterStartupScript(cp, Page.GetType(), getKey(16), tmp, true);
    }

    /// <summary>
    /// JavaScriptのconsole.logを出す。
    /// </summary>
    /// <param name="p">複数出せる</param>
    public void jsl(params string[] p)
    {
        string str = "", tmp = "";
        for (int i = 0; i <= p.Length - 1; i++)
        {
            if (p[i] == null)
                p[i] = "null";
            if (i > 0)
                str += ",";
            tmp = p[i].Replace("\\", "\\\\").Replace("'", "\\'");
            tmp = tmp.Replace("\n", "\\n");
            tmp = tmp.Replace("\r", "\\r");
            tmp = tmp.Replace("\r\n", "\\r\\n");
            str += "'" + tmp + "'";
        }
        js("console.log(" + str + ");");
    }

    #endregion

    #region 文字列
    /// <summary>半角 1 バイト、全角 2 バイトとして、指定された文字列のバイト数を返します。</summary>
    /// <param name="str">バイト数取得の対象となる文字列。</param>
    /// <returns>半角 1 バイト、全角 2 バイトでカウントされたバイト数。</returns>
    public int LenB(string str)
    {
        return System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(str);
    }


    /// <summary>
    /// 文字列の左端から指定したバイト数までの文字列を返します。
    /// </summary>
    /// <param name="str">取り出す元になる文字列</param>
    /// <param name="cnt">取り出すバイト数</param>
    /// <returns>左端から指定されたバイト数までの文字列</returns>
    public string omitLeftB(string str, int cnt)
    {
        return (LenB(str) >= cnt) ? LeftB(str, cnt) : str;
    }

    /// <summary>文字列の左端から指定したバイト数分の文字列を返します。</summary>
    /// <param name="str">取り出す元になる文字列<param>
    /// <param name="cnt">取り出すバイト数</param>
    /// <returns>左端から指定されたバイト数分の文字列</returns>
    public string LeftB(string str, int cnt)
    {
        return MidB(str, 1, cnt);
    }

    /// <summary>文字列の指定されたバイト位置以降のすべての文字列を返します。</summary>
    /// <param name="str">取り出す元になる文字列。</param>
    /// <param name="iStart">取り出しを開始する位置。</param>
    /// <returns>指定されたバイト位置以降のすべての文字列。</returns>
    public string MidB(string str, int iStart)
    {
        System.Text.Encoding hEncoding = System.Text.Encoding.GetEncoding("Shift_JIS");
        byte[] btBytes = hEncoding.GetBytes(str);

        return hEncoding.GetString(btBytes, iStart - 1, btBytes.Length - iStart + 1);
    }

    /// <summary>文字列の指定されたバイト位置から、指定されたバイト数分の文字列を返します。</summary>
    /// <param name="str">取り出す元になる文字列。</param>
    /// <param name="iStart">取り出しを開始する位置。</param>
    /// <param name="cnt">取り出すバイト数。</param>
    /// <returns>指定されたバイト位置から指定されたバイト数分の文字列。</returns>
    public string MidB(string str, int iStart, int cnt)
    {
        System.Text.Encoding hEncoding = System.Text.Encoding.GetEncoding("Shift_JIS");
        byte[] btBytes = hEncoding.GetBytes(str);
        return hEncoding.GetString(btBytes, iStart - 1, cnt);
    }

    /// <summary>文字列の右端から指定されたバイト数分の文字列を返します。</summary>
    /// <param name="str">取り出す元になる文字列。</param>
    /// <param name="cnt">取り出すバイト数。</param>
    /// <returns>右端から指定されたバイト数分の文字列。</returns>
    public string RightB(string str, int cnt)
    {
        System.Text.Encoding hEncoding = System.Text.Encoding.GetEncoding("Shift_JIS");
        byte[] btBytes = hEncoding.GetBytes(str);

        return hEncoding.GetString(btBytes, btBytes.Length - cnt, cnt);
    }

    /// <summary>
    /// HTMLエンコードと改行の置換
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string getHtmlEncode(string str)
    {
        var tmp = "";
        tmp = WebUtility.HtmlEncode(str);
        tmp = str.Replace("<br />", Environment.NewLine);
        return tmp;
    }

    /// <summary>
    /// HTMLデコードと改行の置換
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string getHtmlDecode(string str)
    {
        var tmp = "";
        tmp = str.Replace("\r", "<br />");
        tmp = str.Replace("\n", "<br />");
        tmp = str.Replace(Environment.NewLine, "<br />");
        tmp = WebUtility.HtmlDecode(str);
        return tmp;
    }

    /// <summary>
    /// JSONの値を取得
    /// </summary>
    /// <param name="o">jsonオブジェクト</param>
    /// <returns>値（jsonオブジェクトがない場合は空文字を返す）</returns>
    public string getJSV(object o)
    {
        return Convert.ToString(o as object);
    }

    #endregion

    /// <summary>
    /// 画像がなければ spacer.png にする
    /// </summary>
    /// <param name="imgUrl"></param>
    /// <returns></returns>
    public string getExistImg(string imgUrl)
    {
        try
        {
            WebRequest req = WebRequest.Create(imgUrl);
            WebResponse res = req.GetResponse();
            res.Close();
        }
        catch (Exception ex)
        {
            logError(MethodBase.GetCurrentMethod().Name + ":" + ex.Message);
            logError(imgUrl);
            imgUrl = ResolveUrl("~/Img/spacer.png");
        }
        return imgUrl;
    }


    
    /// <summary>
    /// iPad判定（iPad OS はサーバサイドで判断できないので、厳密にやる必要があるときはJavaScriptで行う）
    /// </summary>
    /// <returns></returns>
    public bool isiPad() {
        var ua = Request.UserAgent;
        return ua.IndexOf("iPad") > -1 || ua.IndexOf("Macintosh") > -1;
    }

    /// <summary>
    /// HTMLヘッダー取得
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public string getHeader(string title)
    {
        string str = "<!DOCTYPE html>";
        str += "<html>";
        str += "<head runat='server'>";
        str += getHeaderAjax(title);
        str += "<title>" + getTitle(title) + "</title>";
        return str;
    }
    public string getHeaderMgt(string title)
    {
        string str = "<!DOCTYPE html>";
        str += "<html>";
        str += "<head runat='server'>";
        str += getHeaderAjaxMgt(title);
        str += "<title>" + getTitle(title) + "</title>";
        return str;
    }

    /// <summary>
    /// HTMLヘッダー取得(Ajax) Ajax Tool kit を使う場合 <head runat='server'> を直接書かないとダメなため
    /// </summary>
    /// <param name="p">互換用（現在未使用）</param>
    /// <returns></returns>
    public string getHeaderAjax(params string[] p)
    {

        String userAgent = Request.UserAgent;

        string str = "<meta charset='UTF-8' />";
        //ページキャッシュを無効（HTTP/1.0）
        str += "<meta http-equiv='pragma' content='no-cache' />";
        //ページキャッシュを無効（HTTP/1.1）
        str += "<meta http-equiv='cache-control' content='no-cache' />";
        //ページの有効期限
        str += "<meta http-equiv='expires' content='0' />";

        //拡大縮小させない
        //str += "<meta name='viewport' content='width=device-width,initial-scale = 1.0,maximum-scale=1.0,user-scalable=0,user-scalable=no' />";
        //拡大縮小を許可する
        //str += "<meta name='viewport' content='width=device-width,initial-scale=1.0' />"
        //デバイスごとにサイズを切り替えて、縮小はさせないようにする
        str += "<meta name='viewport' content='width=device-width,initial-scale=1.0,minimum-scale=1.0'>";

        //iOSでウェブアプリとして表示
        str += "<meta name='apple-mobile-web-app-capable' content='yes' />";
        //str += "<meta name='apple-mobile-web-app-capable' content='no' />";
        //IEは最新版を使う
        str += "<meta http-equiv='X-UA-Compatcntle' content='IE=Edge' />";

        //クロールさせない
        str += "<meta name='ROBOTS' content='NOINDEX, NOFOLLOW' />";
        str += "<meta http-equiv='imagetoolbar' content='no' />";
        str += "<meta http-equiv='imagetoolbar' content='false' />";

        //電話番号の自動リンクを無効にする
        str += "<meta name='format-detection' content='telephone=no' />";

        //favicon
        str += "<link rel='shortcut icon' type='image/vnd.microsoft.icon' href='" + ResolveUrl("~/favicon.ico") + "'>";
        str += "<link rel='icon' type='image/vnd.microsoft.icon' href='" + ResolveUrl("~/favicon.ico") + "'>";

        //iOS icon
        str += "<link rel='apple-touch-icon' sizes='57x57' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-57x57.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='60x60' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-60x60.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='72x72' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-72x72.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='76x76' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-76x76.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='114x114' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-114x114.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='120x120' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-120x120.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='144x144' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-144x144.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='152x152' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-152x152.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='180x180' href='" + ResolveUrl("~/Img/favicons/apple-touch-icon-180x180.png") + "'>";

        /* favicon その他対応。必要になったらコメントアウト戻す。ResolveUrlもつける。
			str += "<link rel='icon' type='image/png' sizes='192x192' href='/Img/favicons/android-chrome-192x192.png'>";
			str += "<link rel='icon' type='image/png' sizes='48x48' href='/Img/favicons/favicon-48x48.png'>";
			str += "<link rel='icon' type='image/png' sizes='96x96' href='/Img/favicons/favicon-96x96.png'>";
			str += "<link rel='icon' type='image/png' sizes='96x96' href='/Img/favicons/favicon-160x160.png'>";
			str += "<link rel='icon' type='image/png' sizes='96x96' href='/Img/favicons/favicon-196x196.png'>";
			str += "<link rel='icon' type='image/png' sizes='16x16' href='/Img/favicons/favicon-16x16.png'>";
			str += "<link rel='icon' type='image/png' sizes='32x32' href='/Img/favicons/favicon-32x32.png'>";
			str += "<link rel='manifest' href='/Img/favicons/manifest.json'>";
			str += "<meta name='msapplication-TileColor' content='#2d88ef'>";
			str += "<meta name='msapplication-TileImage' content='/Img/favicons/mstile-144x144.png'>";
			 */

        //モバイル対応。後で必要になったら作り込む
        //if (mySession.is_mobile == "1")
        //{
        //  //ipad
        //  str += "<link rel='stylesheet' type='text/css' href='" + ResolveUrl("~/cmnMobile.css") + "?r=" + getVersion() + "' />";
        //}
        //else
        //{
        //  //pc
        //  str += "<link rel='stylesheet' type='text/css' href='" + ResolveUrl("~/cmnPc.css") + "?r=" + getVersion() + "' />";
        //}

        //C:\inetpub\wwwroot\withyou

        str += "<link rel='stylesheet' type='text/css' href='" + ResolveUrl("~/cmn.css") + "?r=" + getUpdateTime(Server.MapPath("/cmn.css")) + "' />";

        if (isiPad())
        {
            str += "<link rel='stylesheet' type='text/css' href='" + ResolveUrl("~/cmn_ipad.css") + "?r=" + getUpdateTime(Server.MapPath("/cmn_ipad.css")) + "' />";
        }

        str += "<script src='" + ResolveUrl("~/cmn.js") + "?r=" + getUpdateTime(Server.MapPath("/cmn.js")) + "'></script>";
        str += "<script src='" + ResolveUrl("~/Scripts/svgxuse.js") + "'></script>";

        //JSON非対応用
        str += "<script type='text/javascript'>!window.JSON && document.write(\"<script src='" + ResolveUrl("~/Scripts/json2.js") + "'><\\/script>\")</script>";

        //ロード時スクリプト直書き
        var cmnLoadPath = Request.PhysicalApplicationPath + "Scripts\\cmnLoad.js";
        System.IO.StreamReader sr = new System.IO.StreamReader(
          cmnLoadPath, Encoding.GetEncoding("utf-8"));
        string cmnLoadStr = sr.ReadToEnd();
        sr.Close();
        str += cmnLoadStr;
        return str;
    }


    public string getHeaderAjaxMgt(params string[] p)
    {
        String userAgent = Request.UserAgent;
        string str = "<meta charset='UTF-8' />";
        //ページキャッシュを無効（HTTP/1.0）
        str += "<meta http-equiv='pragma' content='no-cache' />";
        //ページキャッシュを無効（HTTP/1.1）
        str += "<meta http-equiv='cache-control' content='no-cache' />";
        //ページの有効期限
        str += "<meta http-equiv='expires' content='0' />";

        //拡大縮小させない
        //str += "<meta name='viewport' content='width=device-width,initial-scale = 1.0,maximum-scale=1.0,user-scalable=0,user-scalable=no' />";
        //拡大縮小を許可する
        //str += "<meta name='viewport' content='width=device-width,initial-scale=1.0' />"
        //デバイスごとにサイズを切り替えて、縮小はさせないようにする
        str += "<meta name='viewport' content='width=device-width,initial-scale=1.0,minimum-scale=1.0'>";

        //iOSでウェブアプリとして表示
        str += "<meta name='apple-mobile-web-app-capable' content='yes' />";
        //str += "<meta name='apple-mobile-web-app-capable' content='no' />";
        //IEは最新版を使う
        str += "<meta http-equiv='X-UA-Compatcntle' content='IE=Edge' />";

        //クロールさせない
        str += "<meta name='ROBOTS' content='NOINDEX, NOFOLLOW' />";
        str += "<meta http-equiv='imagetoolbar' content='no' />";
        str += "<meta http-equiv='imagetoolbar' content='false' />";

        //電話番号の自動リンクを無効にする
        str += "<meta name='format-detection' content='telephone=no' />";

        //favicon
        str += "<link rel='shortcut icon' type='image/vnd.microsoft.icon' href='" + ResolveUrl("~/faviconMgt.ico") + "'>";
        str += "<link rel='icon' type='image/vnd.microsoft.icon' href='" + ResolveUrl("~/faviconMgt.ico") + "'>";

        //iOS icon
        str += "<link rel='apple-touch-icon' sizes='57x57' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-57x57.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='60x60' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-60x60.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='72x72' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-72x72.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='76x76' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-76x76.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='114x114' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-114x114.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='120x120' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-120x120.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='144x144' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-144x144.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='152x152' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-152x152.png") + "'>";
        str += "<link rel='apple-touch-icon' sizes='180x180' href='" + ResolveUrl("~/Img/faviconsMgt/apple-touch-icon-180x180.png") + "'>";

        /* favicon その他対応。必要になったらコメントアウト戻す。ResolveUrlもつける。
			str += "<link rel='icon' type='image/png' sizes='192x192' href='/Img/faviconsMgt/android-chrome-192x192.png'>";
			str += "<link rel='icon' type='image/png' sizes='48x48' href='/Img/faviconsMgt/favicon-48x48.png'>";
			str += "<link rel='icon' type='image/png' sizes='96x96' href='/Img/faviconsMgt/favicon-96x96.png'>";
			str += "<link rel='icon' type='image/png' sizes='96x96' href='/Img/faviconsMgt/favicon-160x160.png'>";
			str += "<link rel='icon' type='image/png' sizes='96x96' href='/Img/faviconsMgt/favicon-196x196.png'>";
			str += "<link rel='icon' type='image/png' sizes='16x16' href='/Img/faviconsMgt/favicon-16x16.png'>";
			str += "<link rel='icon' type='image/png' sizes='32x32' href='/Img/faviconsMgt/favicon-32x32.png'>";
			str += "<link rel='manifest' href='/Img/faviconsMgt/manifest.json'>";
			str += "<meta name='msapplication-TileColor' content='#2d88ef'>";
			str += "<meta name='msapplication-TileImage' content='/Img/faviconsMgt/mstile-144x144.png'>";
			 */

        //モバイル対応。後で必要になったら作り込む
        //if (mySession.is_mobile == "1")
        //{
        //  //ipad
        //  str += "<link rel='stylesheet' type='text/css' href='" + ResolveUrl("~/cmnMobile.css") + "?r=" + getVersion() + "' />";
        //}
        //else
        //{
        //  //pc
        //  str += "<link rel='stylesheet' type='text/css' href='" + ResolveUrl("~/cmnPc.css") + "?r=" + getVersion() + "' />";
        //}

        //C:\inetpub\wwwroot\withyou

        str += "<link rel='stylesheet' type='text/css' href='" + ResolveUrl("~/cmn.css") + "?r=" + getUpdateTime(Server.MapPath("/cmn.css")) + "' />";
        if (isiPad())
        {
            str += "<link rel='stylesheet' type='text/css' href='" + ResolveUrl("~/cmn_ipad.css") + "?r=" + getUpdateTime(Server.MapPath("/cmn_ipad.css")) + "' />";
        }

        str += "<script src='" + ResolveUrl("~/cmn.js") + "?r=" + getUpdateTime(Server.MapPath("/cmn.js")) + "'></script>";
        str += "<script src='" + ResolveUrl("~/Scripts/svgxuse.js") + "'></script>";

        //JSON非対応用
        str += "<script type='text/javascript'>!window.JSON && document.write(\"<script src='" + ResolveUrl("~/Scripts/json2.js") + "'><\\/script>\")</script>";

        //ロード時スクリプト直書き
        var cmnLoadPath = Request.PhysicalApplicationPath + "Scripts\\cmnLoad.js";
        System.IO.StreamReader sr = new System.IO.StreamReader(
          cmnLoadPath, Encoding.GetEncoding("utf-8"));
        string cmnLoadStr = sr.ReadToEnd();
        sr.Close();
        str += cmnLoadStr;
        return str;
    }


    public string getTitle(string title)
    {
        //タイトル調整
        string pageName = System.IO.Path.GetFileName(Request.Url.ToString());
        if (pageName == "login.aspx" || pageName == "login")
        {
            title = "訪問歯科ナビ WithYou";
        }
        else
        {
            title += " - WithYou";
        }
        return title;
    }

    /// <summary>
    /// 共通タイトルヘッダ取得
    /// </summary>
    /// <param name="n">アイコンナンバー</param>
    /// <param name="t">タイトルテキスト</param>
    /// <returns>共通タイトルヘッダHTML</returns>
    public string getTitleHeader(int n, string t)
    {
        return getTitleHeaderC(n, t, true);
    }

    /// <summary>
    /// オプションメニュー表示可非
    /// </summary>
    /// <returns>bool</returns>
    public string getOptMenu()
    {
        bool check = true;
        string str = "";

        DataTable dt = new DataTable();

        var sqlString = new StringBuilder();
        sqlString.Clear();
        sqlString.AppendLine("select * from mst_group ");
        sqlString.AppendLine("where ");
        sqlString.AppendLine(" group_id = @group_id");

        SqlParameter[] param1 = {
        createSqlParameter("@group_id", SqlDbType.VarChar, sqlTabooChar(mySession.group_id)),
        };
        bool sqlrtn = false;
        sqlrtn = sqlSelectTable(sqlString.ToString(), param1, ref dt);

        if (dt.Rows.Count > 0)
        {
            if (dt.Rows[0]["use_optionmenu"].ToString() != "1")
            {
                check = false;
            }
        }


        string requestPath = Request.Path;

        var hiddeenPathList = new List<String>();
        hiddeenPathList.Add("/confMenu");
        hiddeenPathList.Add("/hospConfig");
        hiddeenPathList.Add("/mst");
        hiddeenPathList.Add("/userConfig");
        hiddeenPathList.Add("/csvImport");
        hiddeenPathList.Add("/outputMenu");
        hiddeenPathList.Add("/dataPrint");
        hiddeenPathList.Add("/documentPrint");
        hiddeenPathList.Add("/formatPrint");
        hiddeenPathList.Add("/dataAndDocumentSend");
        hiddeenPathList.Add("/transactionList");
        hiddeenPathList.Add("/hospSelect");
        hiddeenPathList.Add("/mgt");

        foreach (string path in hiddeenPathList)
        {
            if (requestPath.StartsWith(path))
            {
                check = false;
            }
        }

        if (check)
        {
            //オプションメニューボタン表示
            str += "<div class='btnSizeMMi optMenu' onclick='callOptMenu()'>";
            str += "<img src=" + ResolveUrl("~/Img//navi/icon_opt_menu.svg") + " />";
            str += "</div>";
        }

        return str;
    }

    /// <summary>
    /// オプションメニュー子画面
    /// </summary>
    /// <returns>bool</returns>
    public string getSubWindowOpt()
    {
        bool check = true;
        string str = "";

        DataTable dt = new DataTable();

        var sqlString = new StringBuilder();
        sqlString.Clear();
        sqlString.AppendLine("select * from mst_hosp_gw ");
        sqlString.AppendLine("where ");
        sqlString.AppendLine(" hosp_id = @hosp_id");

        SqlParameter[] param1 = {
            createSqlParameter("@hosp_id", SqlDbType.VarChar, sqlTabooChar(mySession.hosp_id)),
        };
        bool sqlrtn = false;
        sqlrtn = sqlSelectTable(sqlString.ToString(), param1, ref dt);

        if (dt.Rows.Count > 0)
        {
            if (dt.Rows[0]["option_medicine"].ToString() != "1")
            {
                check = false;
            }
        }

        //オプションメニュー子画面
        str += "<div id='pnlSubWindowOptMenu' class='pnlSubWindow' style='visiblity:hidden' onclick=' event.stopPropagation();' >";
        str += "<article>";
        str += "<section>";
        if (check)
        {
            str += "<div id='btnDrugInfo' class='btn btnSizeM' onclick='fncDispDrugInfo();'>"; //onclick='fncDispDrugInfo(\"" + getDrugInfoUrl() + "\");'>";
            str += "<img class='imgOptMenu' src=" + ResolveUrl("~/Img//navi/icon_st_menu07.png") + " />";
        }
        else
        {
            str += "<div id='btnDrugInfo' class='btn btnD btnSizeM'>";
            str += "<img class='imgOptMenu' src=" + ResolveUrl("~/Img//navi/icon_st_menu07_d.png") + " />";
        }

        str += "薬剤情報参照";
        str += "</div>";

        str += "</section>";
        str += "<section>";
        str += "<div id='btnManual' class='btn btnSizeM' onclick='fncDispManual(\"" + getManualUrl() + "\")'>";
        str += "<img class='imgOptMenu' src=" + ResolveUrl("~/Img//navi/icon_st_menu08.png") + " />";
        str += "マニュアル";
        str += "</div>";
        str += "</section>";
        str += "</article>";
        str += "</div>";

        //薬剤情報・マニュアル表示iFrame
        str += "<section id='secOpt' class='dn' onclick='closeSecOpt()'>";
        str += "<div onclick='arguments[0].stopPropagation();'>";
        str += "<div class='title-container'>";
        str += "<div id='divOptTitle'>divOptTitle</div>";
        str += "<div class='divBtnClose' onclick='closeSecOpt();'></div>";
        str += "</div>";
        str += "<iframe id='iOpt' frameborder='0' class='' ></iframe>";

        str += "<div class='footer-container'>";
        str += "</div>";

        str += "</div>";
        str += "</section>";

        return str;
    }

    /// <summary>
    /// 状態ナビボタン表示可非
    /// </summary>
    /// <returns>bool</returns>
    public string getNavi()
    {
        bool check = true;
        string str = "";

        DataTable dt = new DataTable();

        var sqlString = new StringBuilder();
        sqlString.Clear();
        sqlString.AppendLine("select * from mst_hosp_gw ");
        sqlString.AppendLine("where ");
        sqlString.AppendLine(" hosp_id = @hosp_id");

        SqlParameter[] param1 = {
            createSqlParameter("@hosp_id", SqlDbType.VarChar, sqlTabooChar(mySession.hosp_id)),
        };
        bool sqlrtn = false;
        sqlrtn = sqlSelectTable(sqlString.ToString(), param1, ref dt);

        if (dt.Rows.Count > 0)
        {
            if (dt.Rows[0]["option_profile"].ToString() != "1")
            {
                check = false;
            }
        }

        if (!Request.Path.Equals("/planVisit"))
        {
            check = false;
        }

        if (check)
        {
            str += "<div id='btnNavi' class='btn btnSizeM' style='font-size:1.8rem;margin-left:10px;margin-right:0px' onclick='fncDispNavi()'>";
            str += "<img class='imgBtnNavi' src=" + ResolveUrl("~/Img//navi/icon_st_menu06.png") + " />";
            str += "状態把握<br>ナビ";
            str += "</div>";
        }

        return str;
    }

    /// <summary>
    /// 共通タイトルヘッダ（閉じるボタン有無）取得
    /// </summary>
    /// <param name="n"></param>
    /// <param name="t"></param>
    /// <param name="bl">閉じるボタンなしの場合は false </param>
    /// <returns></returns>
    public string getTitleHeaderC(int n, string t, bool bl)
    {
        string str = "<header>";
        str += "<section class='secHeaderTitle'>";

        //オプションメニュー取得
        str += getOptMenu();
        str += "<section class='secHeaderTitle'>";

        str += "<div id='divHeaderIcon' class='icon_title title_item" + n + "'></div>";
        str += "<span id='spnHeaderTitle'>" + t + "</span>";

        //ナビボタン取得
        str += getNavi();

        str += "</section>";
        str += "</section>";
        str += "<section style='height:100%;'>";
        if (string.IsNullOrEmpty(getOptMenu()))
        {
            str += "<div class='fb Uname' style='text-align: right;height:80%;flex-direction: column;margin-top:auto;margin-bottom:auto;margin-right:5px;'><span >";
        }
        else
        {
            str += "<div class='fb Uname UnameOpt' style='text-align: right;height:80%;flex-direction: column;margin-top:auto;margin-bottom:auto;margin-right:5px;'><span >";
        }
        str += mySession.hosp_name;
        str += "</span>";
        str += "<span>ログイン：";
        str += mySession.login_name;
        str += "</span></div>";
        if (bl == true)
        {
            str += "<div id='divBtnHeaderClose' class='btn btnSizeM' onclick='fncClose()'>";
            str += "<svg>";
            str += "<use xmlns:xlink='http://www.w3.org/1999/xlink' xlink:href='" + ResolveUrl("~/Img/icons_system.svg#icon-close") + "'></use>";
            str += "</svg>";
            str += "<span>閉じる</span>";
            str += "</div>";
        }
        str += "</section>";

        str += getSubWindowOpt();
        str += "</header>";
        return str;
    }


    //共通タイトルヘッダ（管理者画面）取得
    public string getTitleHeaderMgt(int n, string t)
    {
        return getTitleHeaderMgt(n, t, true);
    }

    public string getTitleHeaderMgt(int n, string t, bool bl)
    {
        string str = "<header  class='mgtHeader'>";
        str += "<section class='secHeaderTitle'>";
        str += "<div id='divHeaderIcon' class='icon_title title_item" + n + "'></div>";
        str += "<span id='spnHeaderTitle'>" + t + "</span>";
        str += "</section>";
        str += "<section style='height:100%;'>";
        str += "<div class='fb Uname' style='text-align: right;height:80%;flex-direction: column;margin-top:auto;margin-bottom:auto;margin-right:5px;'>";
        //str += "<span>";
        //str += mySession.hosp_name;
        //str += "</span>";
        //str += "<span>ログイン：";
        //str += mySession.login_name;
        //str += "</span>";
        str += "</div>";
        if (bl == true)
        {
            str += "<div id='divBtnHeaderClose' class='btn btnSizeM' onclick='fncClose()'>";
            str += "<svg>";
            str += "<use xmlns:xlink='http://www.w3.org/1999/xlink' xlink:href='" + ResolveUrl("~/Img/icons_system.svg#icon-close") + "'></use>";
            str += "</svg>";
            str += "<span>閉じる</span>";
            str += "</div>";
        }
        str += "</section>";
        str += "</header>";
        return str;
    }




    /// <summary>
    /// 画面遷移共通処理
    /// </summary>
    /// <param name="url">ResolveUrlをかける</param>
    protected void doRedirect(string url)
    {
        //ログ入れる
        Response.Redirect(ResolveUrl(url));
    }

    //必要であれば検証するか
    /*
	  /// <summary>
	  /// パネルオンオフ
	  /// </summary>
	  /// <param name="p"></param>
	  public void turnPnl(params string[] p)
	  {
		try
		{
		  string _id = p[0];
		  int n = 0;
		  if (p.Length == 1)
		  {
			if (((Panel)FindControl(_id + n.ToString())).Visible == true) n = 1;
		  }
		  else
		  {
			n = int.Parse(p[1]);
		  }
		  Panel[] objs = new Panel[3];
		  objs[0] = (Panel)FindControl(_id + n.ToString());
		  objs[1] = (Panel)FindControl(_id + (1 - n).ToString());
		  objs[0].Visible = true;
		  objs[1].Visible = false;
		}
		catch (Exception ex)
		{
		  jsl(ex.Message);
		}
	  }
	  */
    public string getWareki(DateTime d, int flg = 0)
    {
        try
        {
            String wagouName = "";
            String wagouCode = "";
            int yearFrom, monthFrom, dayFrom, yearTo, monthTo, dayTo;
            long fromDate = 0;
            long toDate = 0;
            long originalDate = 0;
            long wagouNumber = 0;
            bool isMatch = false;

            String sql = "SELECT * FROM mst_wagou ORDER BY index_no";
            DataTable dtWareki = new DataTable();
            sqlSelectTable(sql, ref dtWareki);
            originalDate = long.Parse(d.ToString("yyyyMMdd"));


            foreach (DataRow row in dtWareki.Rows)
            {
                //castエラーになるからとりあえずConvert使う
                wagouName = row["wagou_name"].ToString();
                wagouCode = row["wagou_code"].ToString();
                //yearFrom = (int)row["YEAR_FROM"];
                yearFrom = Convert.ToInt32(row["year_from"]);
                monthFrom = Convert.ToInt32(row["month_from"]);
                dayFrom = Convert.ToInt32(row["day_from"]);
                yearTo = Convert.ToInt32(row["year_to"]);
                monthTo = Convert.ToInt32(row["month_to"]);
                dayTo = Convert.ToInt32(row["day_to"]);

                fromDate = long.Parse(string.Format("{0}{1:00}{2:00}", yearFrom, monthFrom, dayFrom));
                toDate = long.Parse(string.Format("{0}{1:00}{2:00}", yearTo, monthTo, dayTo));

                if ((fromDate <= originalDate) && (toDate >= originalDate))
                {
                    wagouNumber = d.Year - yearFrom + 1;
                    isMatch = true;
                    break;
                }
            }

            if (!isMatch)
            {
                return d.ToString("yyyy/MM/dd");
            }

            if (flg == 1)
            {
                return wagouCode + wagouNumber.ToString().PadLeft(2, '0') + "." + d.Month + "." + d.Day;
            }
            else if (flg == 2)
            {
                return wagouName;
            }
            else
            {
                string wagouY = "";
                //if (wagouNumber == 1)
                //{
                //	wagouY = "元";
                //}
                //else
                //{
                //	wagouY = wagouNumber.ToString();
                //}
                wagouY = wagouNumber.ToString();

                return wagouName + wagouY + "年" + d.Month + "月" + d.Day + "日";
                //return wagouName + wagouNumber.ToString().Replace("1", "元") + "年" + d.Month + "月" + d.Day + "日";
            }
        }
        catch (Exception exception)
        {
            return "";
        }
    }

    ///<summary>
    /// 排他チェック
    /// </summary>
    /// <param name="exmUniqueNo"></param>
    /// <param name="userName"></param>
    /// <param name="descrption"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public int exclusiveCheck(string displayName, string patientId, string exmDate, ref string userId)
    {
        try
        {
            string sqlString = "";
            DataTable dt = new DataTable();
            sqlString = "select user_id from tbl_exclusive ";
            sqlString += "where hosp_id = '" + sqlTabooChar(mySession.hosp_id) + "'";
            sqlString += "  AND display_name = '" + sqlTabooChar(displayName) + "'";
            sqlString += "  AND patient_id = '" + sqlTabooChar(patientId) + "'";
            if (string.IsNullOrEmpty(exmDate))
            {
                sqlString += "  and exm_date is null ";
            }
            else
            {
                sqlString += "  and exm_date = '" + exmDate + "'";
            }
            bool sqlrtn = false;
            sqlrtn = sqlSelectTable(sqlString, ref dt);
            if (!sqlrtn)
            {
                logLong(4, cmnName, "exclusive", "排他チェックエラー:" + sqlString, "", "", "");
                return -1;
            }

            if (dt.Rows.Count == 0)
            {
                userId = "";
            }
            else
            {
                userId = dt.Rows[0]["user_id"].ToString();
            }
            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "exclusive", "排他チェックエラー:" + ex.Message, "", "", "");
            return -1;
        }
    }
    public int exclusiveCheck2(string displayName, string patientId, string exmDate, ref string userId, ref string userName)
    {
        try
        {
            string sqlString = "";
            DataTable dt = new DataTable();
            sqlString = "select user_id, user_name from tbl_exclusive ";
            sqlString += "where hosp_id = '" + sqlTabooChar(mySession.hosp_id) + "'";
            sqlString += "  AND display_name = '" + sqlTabooChar(displayName) + "'";
            sqlString += "  AND patient_id = '" + sqlTabooChar(patientId) + "'";
            if (string.IsNullOrEmpty(exmDate))
            {
                sqlString += "  and exm_date is null ";
            }
            else
            {
                sqlString += "  and exm_date = '" + exmDate + "'";
            }
            bool sqlrtn = false;
            sqlrtn = sqlSelectTable(sqlString, ref dt);
            if (!sqlrtn)
            {
                logLong(4, cmnName, "exclusive", "排他チェックエラー:" + sqlString, "", "", "");
                return -1;
            }

            if (dt.Rows.Count == 0)
            {
                userId = "";
                userName = "";
            }
            else
            {
                userId = dt.Rows[0]["user_id"].ToString();
                userName = dt.Rows[0]["user_name"].ToString();
            }
            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "exclusive", "排他チェックエラー:" + ex.Message, "", "", "");
            return -1;
        }
    }

    /// <summary>
    /// 排他設定
    /// </summary>
    /// <param name="exmDate"></param>
    /// <param name="patientId"></param>
    /// <param name="exmUniqueNo"></param>
    /// <param name="caseFlg"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public int exclusiveSet(string displayName, string patientId, string exmDate, string caseFlg, ref string userName)
    {
        try
        {
            //時間切れの排他を削除
            deleteExclusive();

            logLong(3, cmnName, "exclusive", "排他設定:" + caseFlg, patientId, "", exmDate);
            bool sqlrtn = false;
            string sqlString = "";

            switch (caseFlg)
            {
                case "OPEN":        //排他設定
                    DataTable dt = new DataTable();

                    sqlString = "select user_id, user_name,group_id, terminal_id from tbl_exclusive ";
                    sqlString += "where hosp_id = '" + sqlTabooChar(mySession.hosp_id) + "'";
                    sqlString += "  and display_name = '" + sqlTabooChar(displayName) + "'";
                    sqlString += "  and patient_id = '" + sqlTabooChar(patientId) + "'";
                    if (string.IsNullOrEmpty(exmDate))
                    {
                        sqlString += "  and exm_date is null ";
                    }
                    else
                    {
                        sqlString += "  and exm_date = '" + exmDate + "'";
                    }

                    sqlrtn = sqlSelectTable(sqlString, ref dt);
                    if (!sqlrtn)
                    {
                        logLong(4, cmnName, "exclusive", "排他設定エラー:" + sqlString, "", "", "");
                        return -1;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        //該当データ無し、排他設定
                        sqlString = "insert into tbl_exclusive(";
                        sqlString += "group_id,";
                        sqlString += "hosp_id,";
                        sqlString += "user_id,";
                        sqlString += "user_name,";
                        sqlString += "terminal_id,";
                        sqlString += "display_name,";
                        sqlString += "patient_id,";
                        sqlString += "exm_date,";
                        sqlString += "regist_datetime) ";
                        sqlString += "VALUES('" + sqlTabooChar(mySession.group_id) + "',";
                        sqlString += "'" + sqlTabooChar(mySession.hosp_id) + "',";
                        sqlString += "'" + sqlTabooChar(mySession.login_id) + "',";
                        sqlString += "'" + sqlTabooChar(mySession.login_name) + "',";
                        sqlString += "'" + sqlTabooChar(mySession.terminal_id) + "',";
                        sqlString += "'" + sqlTabooChar(displayName) + "',";
                        sqlString += "'" + sqlTabooChar(patientId) + "',";
                        if (string.IsNullOrEmpty(exmDate))
                        {
                            sqlString += "null,";
                        }
                        else
                        {
                            sqlString += "'" + exmDate + "',";
                        }
                        sqlString += "getdate())";

                        Thread.Sleep(1);

                        sqlrtn = sqlInsert(sqlString);
                        if (!sqlrtn)
                        {
                            logLong(4, cmnName, "exclusive", "排他設定エラー:" + sqlString, "", "", "");
                            return -1;
                        }
                    }
                    else
                    {
                        //該当データ有り、排他しているユーザ情報を比較する
                        if (dt.Rows[0]["user_id"].ToString() == mySession.login_id &&
                                        dt.Rows[0]["group_id"].ToString() == mySession.group_id &&
                                        dt.Rows[0]["terminal_id"].ToString() == mySession.terminal_id)
                        {
                            //同一の端末、同一のユーザなので、時間の再設定だけ行う。
                            sqlString = "update tbl_exclusive set regist_datetime = getdate() ";
                            sqlString += "where hosp_id = '" + sqlTabooChar(mySession.hosp_id) + "'";
                            sqlString += "  and display_name = '" + sqlTabooChar(displayName) + "'";
                            sqlString += "  and patient_id = '" + sqlTabooChar(patientId) + "'";
                            sqlString += "  and user_id = '" + sqlTabooChar(mySession.login_id) + "'";
                            sqlString += "  AND group_id = '" + sqlTabooChar(mySession.group_id) + "'";
                            sqlString += "  AND terminal_id = '" + sqlTabooChar(mySession.terminal_id) + "'";
                            if (string.IsNullOrEmpty(exmDate))
                            {
                                sqlString += "  and exm_date is null ";
                            }
                            else
                            {
                                sqlString += "  and exm_date = '" + exmDate + "'";
                            }

                            Thread.Sleep(1);

                            sqlrtn = sqlUpdate(sqlString);
                            if (!sqlrtn)
                            {
                                logLong(4, cmnName, "exclusive", "排他設定エラー:" + sqlString, "", "", "");
                                return -1;

                            }
                        }
                        else
                        {
                            //排他している人を戻す。
                            userName = dt.Rows[0]["user_name"].ToString();
                            return 1;
                        }
                    }
                    break;
                case "CLOSE":   //排他解除
                    sqlString = "delete from tbl_exclusive ";
                    sqlString += "where hosp_id = '" + sqlTabooChar(mySession.hosp_id) + "'";
                    sqlString += "  and display_name = '" + sqlTabooChar(displayName) + "'";
                    sqlString += "  and patient_id = '" + sqlTabooChar(patientId) + "'";
                    sqlString += "  and user_id = '" + sqlTabooChar(mySession.login_id) + "'";
                    sqlString += "  AND group_id = '" + sqlTabooChar(mySession.group_id) + "'";
                    sqlString += "  AND terminal_id = '" + sqlTabooChar(mySession.terminal_id) + "'";
                    if (string.IsNullOrEmpty(exmDate))
                    {
                        sqlString += "  and exm_date is null ";
                    }
                    else
                    {
                        sqlString += "  and exm_date = '" + sqlTabooChar(exmDate) + "'";
                    }
                    sqlrtn = sqlDelete(sqlString);
                    if (!sqlrtn)
                    {
                        logLong(4, cmnName, "exclusive", "排他設定エラー:" + sqlTabooChar(sqlString), "", "", "");
                        return -1;

                    }
                    break;
                case "ADMIN_CLOSE":     //強制解除
                    sqlString = "delete from tbl_exclusive ";
                    sqlString += "where hosp_id = '" + sqlTabooChar(mySession.hosp_id) + "'";
                    sqlString += "  and display_name = '" + sqlTabooChar(displayName) + "'";
                    sqlString += "  and patient_id = '" + sqlTabooChar(patientId) + "'";
                    if (string.IsNullOrEmpty(exmDate))
                    {
                        sqlString += "  and exm_date is null ";
                    }
                    else
                    {
                        sqlString += "  and exm_date = '" + sqlTabooChar(exmDate) + "'";
                    }
                    sqlrtn = sqlDelete(sqlString);
                    if (!sqlrtn)
                    {
                        logLong(4, cmnName, "exclusive", "排他設定エラー:" + sqlTabooChar(sqlString), "", "", "");
                        return -1;

                    }
                    break;

            }

            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "exclusive", "排他設定エラー:" + ex.Message, "", "", "");
            return -1;
        }
    }

    ///<summary>
    /// 排他解除されなかったものを削除（時間経過）
    /// </summary>
    /// <param name="userId"></param>
    /// <remarks></remarks>
    public int deleteExclusive()
    {
        try
        {
            string sqlString = "";
            //１時間以上経過しているものは削除
            sqlString = "delete from tbl_exclusive ";
            sqlString += "where hosp_id = '" + mySession.hosp_id + "'";
            sqlString += "  and regist_datetime <= DATEADD(hh, -1, getdate())";
            bool sqlrtn = false;
            sqlrtn = sqlDelete(sqlString);
            if (!sqlrtn)
            {
                logLong(4, cmnName, "exclusive", "排他削除エラー:" + sqlString, "", "", "");
                return -1;
            }
            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "exclusive", "排他削除エラー:" + ex.Message, "", "", "");
            return -1;
        }
    }


    /// <summary>
    /// 特定画面のユーザー排他を全削除
    /// </summary>
    /// <param name="displayName"></param>
    /// <returns></returns>
    public int deleteUserExclusive(string displayName)
    {
        string sql = "delete from tbl_exclusive";
        sql += string.Format(" where hosp_id = '{0}'", sqlTabooChar(mySession.hosp_id));
        sql += string.Format(" and group_id='{0}'", sqlTabooChar(mySession.group_id));
        sql += string.Format(" and user_id='{0}'", sqlTabooChar(mySession.login_id));
        sql += string.Format(" and display_name='quide'", sqlTabooChar(displayName));
        if (!sqlDelete(sql))
        {
            logLong(4, cmnName, "exclusive", "ユーザー排他削除エラー:" + sqlTabooChar(sql), "", "", "");
            return -1;
        }
        return 0;
    }

    /// <summary>
    /// TBL_SESSION登録
    /// </summary>
    /// <returns>-1:エラー,0:正常終了,1:セッション有り</returns>
    public int chkTblSession()
    {
        try
        {
            bool sqlrtn = false;
            string sql = "";
            //ログイン時

            //セッションチェック
            sql = "select * from tbl_session ";     //mgtのみ。本体のLoginチェックと同様にする。今後使用しないようにする
            sql += "where user_id = '" + sqlTabooChar(mySession.login_id) + "' ";
            sql += "and group_id = '" + sqlTabooChar(mySession.group_id) + "' ";

            //初期化
            DataTable dt = new DataTable();
            sqlrtn = sqlSelectTable(sql, ref dt);
            if (!sqlrtn)
            {
                logLong(4, cmnName, "chkTblSession", "tbl_session select error", "", "", "");
                return -1;
            }
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["terminal_id"].ToString() == mySession.terminal_id)
                {

                    if (updateTblSession() < 0)
                    {
                        return -1;
                    }
                }
                else
                {
                    //他の端末のセッション有る場合、ログの最終時間から60分立っているものを削除する
                    sql = "select max(do_date) as dodate from tbl_operation_log ";
                    sql += "where login_id = '" + sqlTabooChar(mySession.login_id) + "' ";
                    sql += "and group_id = '" + sqlTabooChar(mySession.group_id) + "' ";
                    sql += "and DATEADD(minute, 60, do_date) > getdate() ";
                    sql += "and kind = '3' ";

                    //初期化
                    DataTable dtope = new DataTable();
                    sqlrtn = sqlSelectTable(sql, ref dtope);
                    if (!sqlrtn)
                    {
                        logLong(4, cmnName, "chkTblSession", "tbl_operation_log select error", "", "", "");
                        return -1;
                    }
                    if (dtope.Rows.Count > 0 & !string.IsNullOrEmpty(dtope.Rows[0][0].ToString()))
                    {
                        //まだ使用中
                        return 1;

                    }
                    else
                    {
                        //60分立っているのでセッション削除する
                        sql = "delete from tbl_session ";       //mgtのみ。今後使用しないようにする
                        sql += "where user_id = '" + sqlTabooChar(mySession.login_id) + "' ";
                        sql += "and group_id = '" + sqlTabooChar(mySession.group_id) + "' ";
                        sqlrtn = sqlTran(sql);
                        if (!sqlrtn)
                        {
                            logLong(4, cmnName, "chkTblSession", "delete tbl_session error", "", "", "");
                            return -1;
                        }
                        //tbl_sessionへ登録する
                        if (insertTblSession() < 0)
                        {
                            return -1;
                        }


                    }

                }

            }
            else
            {
                //tbl_sessionへ登録する
                if (insertTblSession() < 0)
                {
                    return -1;
                }

            }

            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "chkTblSession", ex.Message, "", "", "");
            return -1;
        }
    }

    //tbl_session 更新
    public int updateTblSession()
    {
        try
        {
            bool sqlrtn = false;
            string sqlString = "";
            sqlString = "update tbl_session ";      //メディア管理者のみ使用。必要か？session_flgは2にする。
            sqlString += "set terminal_id = '" + sqlTabooChar(mySession.terminal_id) + "', ";
            sqlString += "regist_datetime = getdate() ";
            sqlString += "where ";
            sqlString += " user_id = '" + sqlTabooChar(mySession.login_id) + "' ";
            sqlString += " and group_id = '" + sqlTabooChar(mySession.group_id) + "' ";

            sqlrtn = sqlTran(sqlString);
            if (!sqlrtn)
            {
                //js("alertmsg('エラー', 'セッション情報登録エラー。', 'ＯＫ', \"alertclose('');\")");
                logLong(4, cmnName, "updateTblSession", "tbl_session update error", "", "", "");
                return -1;
            }


            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "updateTblSession", ex.Message, "", "", "");
            return -1;
        }

    }


    //tbl_session登録
    public int insertTblSession()
    {
        try
        {
            bool sqlrtn = false;

            string sqlString = "";
            sqlString = "insert into tbl_session( ";    //管理者だけ暫定処置
            sqlString += "user_id, ";
            sqlString += "user_name, ";
            sqlString += "group_id, ";
            sqlString += "terminal_id, ";
            sqlString += "session_flg, ";   //暫定
            sqlString += "media_auth, ";
            sqlString += "regist_datetime) ";
            sqlString += "values( ";
            sqlString += "'" + sqlTabooChar(mySession.login_id) + "', ";
            sqlString += "'" + sqlTabooChar(mySession.login_name) + "', ";
            sqlString += "'" + sqlTabooChar(mySession.group_id) + "', ";
            sqlString += "'" + sqlTabooChar(mySession.terminal_id) + "', ";
            sqlString += "'1', ";   //暫定
            sqlString += "'" + sqlTabooChar(AuthKeyLib.GenerateAuthKey(64)) + "', ";
            sqlString += "getdate()) ";

            sqlrtn = sqlTran(sqlString);
            if (!sqlrtn)
            {
                logLong(4, cmnName, "insertTblSession", "insert tbl_session error", "", "", "");
                return -1;
            }
            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "insertTblSession", "insertTblSession() " + ex.Message, "", "", "");
            return -1;
        }
    }

    //TBL_SESSION削除
    public int delTblSession()
    {
        try
        {
            string sql = "";
            bool sqlrtn = false;

            //ログアウト時
            sql = "delete tbl_session ";    //管理者だけ暫定処置
            sql += "where ";
            sql += " user_id = '" + sqlTabooChar(mySession.login_id) + "' ";
            sql += " and group_id = '" + sqlTabooChar(mySession.group_id) + "' ";
            sql += " and terminal_id = '" + sqlTabooChar(mySession.terminal_id) + "' ";
            sql += " and session_flg = '1' ";   //暫定

            sqlrtn = sqlTran(sql);
            if (!sqlrtn)
            {
                logLong(4, cmnName, "delTblSession", "delTblSession() delete error", "", "", "");
                return -1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "delTblSession", ex.Message, "", "", "");
            return -1;
        }
    }

    /// <summary>
    /// 多重ログインチェック
    /// </summary>
    /// <param name="txtUserId"></param>
    /// <param name="txtHospId"></param>
    /// <param name="txtGroupId"></param>
    /// <param name="flgLogin">login:true checkSession:false default:false</param>
    /// <returns>1：多重ログイン 0：多重ではない（ログイン可能） -1:判定エラー</returns>
    public int ChkMultiLogin(string txtUserId, string txtHospId, string txtGroupId, string txtTerminalId, string flg, bool flgLogin = false)
    {
        logLong(2, cmnName, "multiLogin", "多重ログイン判定開始", "", "", "");

        bool sqlrtn = false;

        System.Text.StringBuilder sql = new System.Text.StringBuilder();

        try
        {
            sql.Clear();
            sql.AppendLine("delete ");
            sql.AppendLine("from ");
            sql.AppendLine("  tbl_session ");
            sql.AppendLine("where ");
            sql.AppendLine(string.Format("  group_id = '{0}' ", sqlTabooChar(txtGroupId)));
            sql.AppendLine(string.Format("  and user_id = '{0}' ", sqlTabooChar(txtUserId)));
            sql.AppendLine(string.Format("  and session_flg = '{0}' ", sqlTabooChar(flg)));
            sql.AppendLine("  and isNull(terminal_id,'') = '' ");
            //jsl(sql.ToString());

            sqlrtn = sqlTran(sql.ToString());
            if (!sqlrtn)
            {
                logLong(4, cmnName, "multiLogin", "delete tbl_session error:" + sql, "", "", "");
                return -1;
            }

        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "multiLogin", "INSERT tbl_session error:" + ex.Message, "", "", "");
            return -1;
        }

        string txtKey = string.Format("{0}_{1}_{2}_{3}", txtUserId, txtHospId, txtGroupId, txtTerminalId);

        //Timeoutしているsessionを消す
        string sqlString = "";
        sqlString = "delete tbl_session ";
        sqlString += " where regist_datetime < DATEADD(MI, -60, getdate()) ";
        sqlrtn = sqlTran(sqlString);
        if (!sqlrtn)
        {
            //js("alertmsg('エラー', 'セッション情報登録エラー。', 'ＯＫ', \"alertclose('');\")");
            logLong(4, cmnName, "deleteSessionTime", "tbl_session delete error", "", "", "");
            return -1;
        }

        DataTable dt = new DataTable();
        try
        {
            sql.Clear();
            sql.AppendLine("SELECT * FROM tbl_session ");
            sql.AppendLine(string.Format(" WHERE user_id  = '{0}' ", sqlTabooChar(txtUserId)));
            sql.AppendLine(string.Format("   AND group_id = '{0}' ", sqlTabooChar(txtGroupId)));
            sql.AppendLine(string.Format("   AND session_flg = '{0}' ", sqlTabooChar(flg)));    //0:通常　1：グループ管理者　2：メディア管理者
                                                                                                //sql.AppendLine(string.Format("   AND hosp_id  = '{0}' ", sqlTabooChar(txtHospId)));
                                                                                                //jsl(sql);

            sqlrtn = sqlSelectTable(sql.ToString(), ref dt);
            if (!sqlrtn)
            {
                logLong(4, cmnName, "multiLogin", "SELECT tbl_session error:" + txtKey, "", "", "");
                return -1;
            }
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "multiLogin", "SELECT tbl_session error:" + ex.Message, "", "", "");
            return -1;
        }


        DataTable dt2 = new DataTable();
        try
        {
            sql.Clear();
            sql.AppendLine("SELECT * FROM tbl_session ");
            //sql.AppendLine(string.Format(" WHERE group_id  = '{0}' ", sqlTabooChar(txtGroupId)));
            //sql.AppendLine(string.Format("   AND terminal_id = '{0}' ", sqlTabooChar(txtTerminalId)));
            sql.AppendLine(string.Format(" WHERE terminal_id = '{0}' ", sqlTabooChar(txtTerminalId)));
            sql.AppendLine(string.Format("   AND session_flg = '{0}' ", sqlTabooChar(flg)));    //0:通常　1：グループ管理者　2：メディア管理者

            //sql.AppendLine(string.Format("   AND hosp_id  = '{0}' ", sqlTabooChar(txtHospId)));
            sqlrtn = sqlSelectTable(sql.ToString(), ref dt2);
            if (!sqlrtn)
            {
                logLong(4, cmnName, "multiLogin", "SELECT tbl_session error2:" + txtKey, "", "", "");
                return -1;
            }
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "multiLogin", "SELECT tbl_session error2:" + ex.Message, "", "", "");
            return -1;
        }


        if (dt.Rows.Count == 0 && dt2.Rows.Count == 0)
        {
            //どこにもないので新規でSessionを追加してLogin
            try
            {
                sql.Clear();
                sql.AppendLine("INSERT INTO tbl_session ");
                sql.AppendLine("     ( user_id ");
                sql.AppendLine("     , group_id ");
                sql.AppendLine("     , hosp_id ");
                sql.AppendLine("     , session_id ");
                sql.AppendLine("     , terminal_id ");
                sql.AppendLine("     , session_flg ");
                sql.AppendLine("     , media_auth ");
                //sql.AppendLine("     , product_kbn ");
                sql.AppendLine("     , regist_datetime ");
                sql.AppendLine(") VALUES  ");
                sql.AppendLine(string.Format("( '{0}' ", sqlTabooChar(txtUserId)));
                sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(txtGroupId)));
                sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(txtHospId)));
                sql.AppendLine(string.Format(", '{0}' ", Session.SessionID.ToString()));
                sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(txtTerminalId)));
                sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(flg)));
                sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(AuthKeyLib.GenerateAuthKey(64))));
                //sql.AppendLine(", '1' ");
                sql.AppendLine(", getdate()) ");
                //jsl(sql.ToString());

                sqlrtn = sqlTran(sql.ToString());
                if (!sqlrtn)
                {
                    logLong(4, cmnName, "multiLogin", "INSERT tbl_session error:" + sql, "", "", "");
                    return -1;
                }

            }
            catch (Exception ex)
            {
                logLong(4, cmnName, "multiLogin", "INSERT tbl_session error:" + ex.Message, "", "", "");
                return -1;
            }
            //jsl("新KEY登録");
            return 0;

        }
        else if (dt.Rows.Count == 0 && dt2.Rows.Count != 0)
        {
            //同じ端末で既に別ユーザでLoginしている　
            return 2;
        }
        else if (dt.Rows.Count != 0 && dt2.Rows.Count == 0)
        {
            //別の端末で既に同じユーザがLoginしている　エラーとする
            return 1;
        }
        else
        {
            if (dt.Rows[0]["terminal_id"].ToString() == txtTerminalId)
            {
                //同じ端末で既に同じユーザがLoginしている　再LoginとしてLoginする（同じ医院IDでLogin）
                try
                {
                    sql.Clear();
                    sql.AppendLine("UPDATE tbl_session ");
                    sql.AppendLine("SET regist_datetime = getdate() ");
                    sql.AppendLine(string.Format("  ,  session_id = '{0}' ", Session.SessionID.ToString()));
                    sql.AppendLine(string.Format("WHERE user_id  = '{0}' ", sqlTabooChar(txtUserId)));
                    sql.AppendLine(string.Format("  AND group_id = '{0}' ", sqlTabooChar(txtGroupId)));
                    sql.AppendLine(string.Format("  AND session_flg = '{0}' ", sqlTabooChar(flg)));
                    //sql.AppendLine(string.Format("  AND hosp_id  = '{0}' ", sqlTabooChar(txtHospId)));
                    //jsl(sql);

                    sqlrtn = sqlTran(sql.ToString());
                    if (!sqlrtn)
                    {
                        logLong(4, cmnName, "multiLogin", "UPDATE tbl_session error:" + sql, "", "", "");
                        return -1;
                    }

                    mySession.hosp_id = dt.Rows[0]["hosp_id"].ToString();

                    return 0;
                }
                catch (Exception ex)
                {
                    logLong(4, cmnName, "multiLogin", "UPDATE tbl_session error:" + ex.Message, "", "", "");
                    return -1;
                }
            }
            else
            {
                //別の端末で既に同じユーザがLoginしている　エラーとする
                return 1;
            }
        }


        //if (flgLogin)
        //{
        //	if (dt.Rows.Count == 0)
        //	{
        //		//jsl("Keyがないので多重ログインではない");
        //		try
        //		{
        //			sql.Clear();
        //			sql.AppendLine("INSERT INTO tbl_session ");
        //			sql.AppendLine("     ( user_id ");
        //			sql.AppendLine("     , group_id ");
        //			sql.AppendLine("     , hosp_id ");
        //			sql.AppendLine("     , session_id ");
        //			sql.AppendLine("     , terminal_id ");
        //			sql.AppendLine("     , session_flg ");
        //			sql.AppendLine("     , regist_datetime ");
        //			sql.AppendLine(") VALUES  ");
        //			sql.AppendLine(string.Format("( '{0}' ", sqlTabooChar(txtUserId)));
        //			sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(txtGroupId)));
        //			sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(txtHospId)));
        //			sql.AppendLine(string.Format(", '{0}' ", Session.SessionID.ToString()));
        //			sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(txtTerminalId)));
        //			sql.AppendLine(string.Format(", '{0}' ", sqlTabooChar(flg)));
        //			sql.AppendLine(", SYSDATETIME()) ");
        //			//jsl(sql);

        //			sqlrtn = sqlTran(sql.ToString());
        //			if (!sqlrtn)
        //			{
        //				logLong(4, cmnName, "multiLogin", "INSERT tbl_session error:" + sql, "", "", "");
        //				return -1;
        //			}

        //		}
        //		catch (Exception ex)
        //		{
        //			logLong(4, cmnName, "multiLogin", "INSERT tbl_session error:" + ex.Message, "", "", "");
        //			return -1;
        //		}
        //		//jsl("新KEY登録");
        //		return 0;
        //	}
        //}
        //else
        //{
        //	if (dt.Rows.Count == 0)
        //	{
        //		//jsl("ログイン後なのでエントリが存在しないのはおかしい");
        //		logLong(4, cmnName, "multiLogin", "SELECT tbl_session error:" + txtKey, "", "", "");
        //		return -1;
        //	}
        //}

        ////jsl("セッション情報登録日時取得 TZ:UTC");
        //DateTime dtRegistDatetime = (DateTime)dt.Rows[0]["regist_datetime"];

        ////jsl("取得した日時にセッションタイムアウト及び固定値（10分）を加算");
        //dtRegistDatetime = dtRegistDatetime.AddMinutes((double)Session.Timeout + (double)10.0);
        ////jsl(dtRegistDatetime.ToString("yyyy/MM/dd HH:mm:ss.fffffff"));
        //if (dtRegistDatetime < DateTime.UtcNow) // UTC日時取得
        //{
        //	//jsl("すでにセッションタイムアウトしているので情報更新");
        //	try
        //	{
        //		sql.Clear();
        //		sql.AppendLine("UPDATE tbl_session ");
        //		sql.AppendLine("SET regist_datetime = SYSDATETIME() ");
        //		sql.AppendLine(string.Format("  ,  terminal_id = '{0}' ", txtTerminalId));
        //		sql.AppendLine(string.Format("  ,  session_id = '{0}' ", Session.SessionID.ToString()));
        //		sql.AppendLine(string.Format("WHERE user_id  = '{0}' ", sqlTabooChar(txtUserId)));
        //		sql.AppendLine(string.Format("  AND group_id = '{0}' ", sqlTabooChar(txtGroupId)));
        //		sql.AppendLine(string.Format("  AND session_flg = '{0}' ", sqlTabooChar(flg)));
        //		//sql.AppendLine(string.Format("  AND hosp_id  = '{0}' ", sqlTabooChar(txtHospId)));
        //		//jsl(sql);

        //		sqlrtn = sqlTran(sql.ToString());
        //		if (!sqlrtn)
        //		{
        //			logLong(4, cmnName, "multiLogin", "UPDATE tbl_session error:" + sql, "", "", "");
        //			return -1;
        //		}
        //		return 0;
        //	}
        //	catch (Exception ex)
        //	{
        //		logLong(4, cmnName, "multiLogin", "UPDATE tbl_session error:" + ex.Message, "", "", "");
        //		return -1;
        //	}
        //}
        //else
        //{
        //	string dtTerminalId = dt.Rows[0]["terminal_id"].ToString();
        //	//jsl("dtTerminalId:" + dtTerminalId);
        //	//jsl("args TerminalId:" + txtTerminalId);
        //	if (dtTerminalId == txtTerminalId)
        //	{
        //		//jsl("ターミナルIDが同じ（再ログイン）なので情報更新");
        //		try
        //		{
        //			sql.Clear();
        //			sql.AppendLine("UPDATE tbl_session ");
        //			sql.AppendLine("SET regist_datetime = SYSDATETIME() ");
        //			sql.AppendLine(string.Format("  ,  session_id = '{0}' ", Session.SessionID.ToString()));
        //			sql.AppendLine(string.Format("WHERE user_id  = '{0}' ", sqlTabooChar(txtUserId)));
        //			sql.AppendLine(string.Format("  AND group_id = '{0}' ", sqlTabooChar(txtGroupId)));
        //			sql.AppendLine(string.Format("  AND session_flg = '{0}' ", sqlTabooChar(flg)));
        //			//sql.AppendLine(string.Format("  AND hosp_id  = '{0}' ", sqlTabooChar(txtHospId)));
        //			//jsl(sql);

        //			sqlrtn = sqlTran(sql.ToString());
        //			if (!sqlrtn)
        //			{
        //				logLong(4, cmnName, "multiLogin", "UPDATE tbl_session error:" + sql, "", "", "");
        //				return -1;
        //			}

        //			mySession.hosp_id = dt.Rows[0]["hosp_id"].ToString();

        //			return 0;
        //		}
        //		catch (Exception ex)
        //		{
        //			logLong(4, cmnName, "multiLogin", "UPDATE tbl_session error:" + ex.Message, "", "", "");
        //			return -1;
        //		}
        //	}
        //}

        //jsl("多重ログインと判断");
        //logLong(4, cmnName, "multiLogin", "多重ログイン:" + txtKey, "", "", "");
        //return 1;
    }

    /// <summary>
    /// ログアウト
    /// </summary>
    public void doLogout(int mode = 0, string flg = "")
    {

        bool mediadmin = (getSession("MEDIA_USER") == "1") ? true :
                         (getSession("MEDIA_ADMIN") == "1") ? true :
                         false;

        System.Text.StringBuilder sql = new System.Text.StringBuilder();
        bool sqlrtn = false;
        switch (mode)
        {
            case (1):
                try
                {
                    string txtKey = string.Format("{0}_{1}_{2}", mySession.login_id, mySession.group_id, mySession.hosp_id);
                    logLong(2, cmnName, "doLogout", "DELETE tbl_session:" + txtKey, "", "", "");

                    sql.Clear();
                    sql.AppendLine("DELETE tbl_session ");
                    sql.AppendLine(string.Format("WHERE user_id  = '{0}' ", sqlTabooChar(mySession.login_id)));
                    sql.AppendLine(string.Format("  AND group_id = '{0}' ", sqlTabooChar(mySession.group_id)));
                    sql.AppendLine(string.Format("  AND session_flg = '{0}' ", sqlTabooChar(flg)));
                    //sql.AppendLine(string.Format("  AND hosp_id  = '{0}' ", sqlTabooChar(mySession.hosp_id)));
                    sqlrtn = sqlTran(sql.ToString());
                    if (!sqlrtn)
                    {
                        logLong(4, cmnName, "doLogout", "DELETE tbl_session error:" + sql, "", "", "");
                    }
                }
                catch (Exception ex)
                {
                    logLong(4, cmnName, "doLogout", "DELETE tbl_session error:" + ex.Message, "", "", "");
                    break;
                }
                Session.Abandon();

                if (flg == "1")
                {
                    if (mediadmin)
                    {
                        //メディア医院管理保守
                        doRedirect("~/media/mgt/mglogin.aspx");
                    }
                    else
                    {
                        //管理者
                        doRedirect("~/Mgt/glogin.aspx");
                    }
                }
                else
                {
                    if (mediadmin)
                    {
                        //メディア医院管理保守
                        doRedirect("~/media/mlogin.aspx");
                    }
                    else
                    {
                        //通常
                        doRedirect("~/login.aspx");
                    }
                }
                //doRedirect("~/login.aspx");

                break;
            default:
                Session.Abandon();
                if (flg == "1")
                {
                    if (mediadmin)
                    {
                        //メディア医院管理保守
                        doRedirect("~/media/mgt/mglogin.aspx");
                    }
                    else
                    {
                        //管理者
                        doRedirect("~/Mgt/glogin.aspx");
                    }
                }
                else
                {
                    if (mediadmin)
                    {
                        //メディア医院管理保守
                        doRedirect("~/media/mlogin.aspx");
                    }
                    else
                    {
                        //通常
                        doRedirect("~/login.aspx");
                    }
                }
                //doRedirect("~/login.aspx");
                break;
        }
    }

    /// <summary>
    /// ログイン履歴
    /// </summary>
    public void loginHistory(string flg)
    {
        System.Text.StringBuilder sqlString = new System.Text.StringBuilder();
        SqlParameter[] param = new SqlParameter[2];
        try
        {
            //端末情報取得
            sqlString.Clear();
            sqlString.AppendLine("select terminal_name, remarks from mst_terminal ");
            sqlString.AppendLine(" where group_id = @GROUPID ");
            sqlString.AppendLine("   and terminal_id = @TERMID ");
            param[0] = createSqlParameter("@GROUPID", SqlDbType.VarChar, sqlTabooChar(mySession.group_id));
            param[1] = createSqlParameter("@TERMID", SqlDbType.VarChar, sqlTabooChar(mySession.terminal_id));

            DataTable dt = new DataTable();
            try
            {
                if (!sqlSelectTable(sqlString.ToString(), param, ref dt))
                {
                    logLong(4, cmnName, "loginHistory", "select mst_terminal error", "", "", "");
                }
            }
            catch (Exception ex)
            {
                logLong(4, cmnName, "loginHistory", "select mst_terminal error: " + ex.Message, "", "", "");
            }
            string termName = "";
            string remarks = "";
            if (dt.Rows.Count != 0)
            {
                termName = dt.Rows[0]["terminal_name"].ToString();
                remarks = dt.Rows[0]["remarks"].ToString();
            }



            //ログイン履歴保存
            sqlString.Clear();
            sqlString.AppendLine("insert into tbl_login_history( ");
            sqlString.AppendLine("    terminal_id ");
            sqlString.AppendLine("  , login_date ");
            sqlString.AppendLine("  , group_id ");
            sqlString.AppendLine("  , user_id ");
            sqlString.AppendLine("  , user_name ");
            sqlString.AppendLine("  , hosp_id ");
            sqlString.AppendLine("  , terminal_name ");
            sqlString.AppendLine("  , remarks ");
            sqlString.AppendLine("  , function_flg ");
            sqlString.AppendLine(") ");
            sqlString.AppendLine("VALUES ( ");
            sqlString.AppendLine("    @TERMID ");
            //sqlString.AppendLine("  , dbo.SYSDATE() ");
            sqlString.AppendLine("  , getdate() ");
            sqlString.AppendLine("  , @GROUPID ");
            sqlString.AppendLine("  , @USERID ");
            sqlString.AppendLine("  , @USERNAME ");
            sqlString.AppendLine("  , @HOSPID ");
            sqlString.AppendLine("  , @TERMNAME ");
            sqlString.AppendLine("  , @REMARKS ");
            sqlString.AppendLine("  , @FLG");
            sqlString.AppendLine(") ");

            Array.Resize(ref param, 8);
            param[0] = createSqlParameter("@TERMID", SqlDbType.VarChar, sqlTabooChar(mySession.terminal_id));
            param[1] = createSqlParameter("@GROUPID", SqlDbType.VarChar, sqlTabooChar(mySession.group_id));
            param[2] = createSqlParameter("@USERID", SqlDbType.VarChar, sqlTabooChar(mySession.login_id));
            param[3] = createSqlParameter("@USERNAME", SqlDbType.VarChar, sqlTabooChar(mySession.login_name));
            param[4] = createSqlParameter("@HOSPID", SqlDbType.VarChar, sqlTabooChar(mySession.hosp_id));
            param[5] = createSqlParameter("@TERMNAME", SqlDbType.VarChar, sqlTabooChar(termName));
            param[6] = createSqlParameter("@REMARKS", SqlDbType.VarChar, sqlTabooChar(remarks));
            param[7] = createSqlParameter("@FLG", SqlDbType.VarChar, sqlTabooChar(flg));

            if (!sqlUpdate(sqlString.ToString(), param))
            {
                logLong(4, cmnName, "loginHistory", "insert into tbl_login_history error", "", "", "");
            }
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "loginHistory", "insert into tbl_login_history error:" + ex.Message, "", "", "");
        }
    }



    #region 定型文

    /// <summary>
    /// 定型文取得
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void doSentenceLoad(object sender, EventArgs e)
    {
        HiddenField hdn = Page.FindControl("hdnSentenceLoad") as HiddenField;
        HiddenField hdnNavi = Page.FindControl("hdnSentenceLoadNavi") as HiddenField;
        HiddenField hdnPatientId = Page.FindControl("hdnSentenceLoadPatientId") as HiddenField;

        DataTable dt = new DataTable();

        ClearOptKinds();

        if (!string.IsNullOrEmpty(hdnNavi.Value) && hdn.Value.ToLower() == "disease")
        {
            dt = GetSentenceNaviDisease(hdnPatientId.Value);
            SetOptKindsNaviDisease();
        }
        else if (!string.IsNullOrEmpty(hdnNavi.Value) && hdn.Value.ToLower() == "medicine")
        {
            dt = GetSentenceNaviMedicine(hdnPatientId.Value);
            SetOptKindsNaviMedicine();
        }
        else
        {
            string[] classIds = hdn.Value.Split(new[] { "," }, StringSplitOptions.None);

            string sql = "select s.sentence";
            sql += ",c.class2_name";
            sql += " from mst_sentence s";
            sql += " left join mst_sentence_class c";
            sql += " on s.class1_id = c.class1_id";
            sql += " and s.class2_id = c.class2_id";
            sql += string.Format(" where s.hosp_id='{0}'", sqlTabooChar(mySession.hosp_id));
            sql += string.Format(" and s.class1_id='{0}'", sqlTabooChar(classIds[0]));
            sql += string.Format(" and s.class2_id='{0}'", sqlTabooChar(classIds[1]));
            sql += " order by s.index_no asc";

            sqlSelectTable(sql, ref dt);

            if (dt.Rows.Count == 0)
            {
                sql = "select class2_name";
                sql += " from mst_sentence_class";
                sql += string.Format(" where class1_id='{0}'", sqlTabooChar(classIds[0]));
                sql += string.Format(" and class2_id='{0}'", sqlTabooChar(classIds[1]));
                sqlSelectTable(sql, ref dt);

                if (dt.Rows.Count == 0) return;
            }
        }

        dt.Columns.Add("shortSentence", Type.GetType("System.String"));
        for (var i = 0; i < dt.Rows.Count; i++)
        {
            string s = dt.Rows[i]["sentence"].ToString();
            s = s.Replace(Environment.NewLine, "　");
            s = s.Replace("</br>", "　");
            s = s.Replace("<br>", "　");
            //if (s.Length > 33) s = s.Substring(0, 30) + "...";
            dt.Rows[i]["shortSentence"] = s;
        }

        ViewState["dtSentence"] = dt;
        DataPager p = Page.FindControl("dpSentence") as DataPager;
        NumericPagerField pagerField = p.Fields[1] as NumericPagerField;
        CommandEventArgs args = new CommandEventArgs("0", "");
        pagerField.HandleEvent(args);

        js(String.Format("initSentence('{0}')", dt.Rows[0]["class2_name"].ToString()));
        js("setSentenceMax()");
    }

    private void ClearOptKinds()
    {
        var rdoli = Page.FindControl("rdoliOptKindsContainer") as RadioButtonList;
        rdoli.Items.Clear();
    }

    /// <summary>
    /// 現病/既往歴区分
    /// </summary>
    private static class MedicalHistories
    {
        public const string PresentDiseaseConsulted = "現病(対診済)";
        public const string PresentDisease = "現病";
        public const string MedicalHistory = "既往歴・未選択";

        public static IReadOnlyList<string> GetValues()
        {
            return new[] {
        PresentDiseaseConsulted,
        PresentDisease,
        MedicalHistory,
      };
        }
    }

    private DataTable GetSentenceNaviDisease(string patientId)
    {
        var sql = new StringBuilder();
        sql.AppendLine("select");
        sql.AppendLine("case");
        sql.AppendLine("  when medical_history is null");
        sql.AppendLine("    then '既往歴・未選択'");
        sql.AppendLine("  when medical_history = '既往歴'");
        sql.AppendLine("    then '既往歴・未選択' ");
        sql.AppendLine("  else medical_history");
        sql.AppendLine("  end kind");
        sql.AppendLine(", disease_name sentence");
        sql.AppendLine(", '疾患名' class2_name");
        sql.AppendLine("from");
        sql.AppendLine("(");
        sql.AppendLine("  select");
        sql.AppendLine("  medical_history");
        sql.AppendLine("  , disease_name");
        sql.AppendLine("  , ROW_NUMBER() over(");
        sql.AppendLine("      order by");
        sql.AppendLine("        medical_history");
        sql.AppendLine("        , convert(date, regist_date) desc");
        sql.AppendLine("        , measure_date desc");
        sql.AppendLine("        , seq_no desc");
        sql.AppendLine("        , regist_date");
        sql.AppendLine("    ) sort_order");
        sql.AppendLine("  , ROW_NUMBER() over(");
        sql.AppendLine("      partition by");
        sql.AppendLine("        medical_history");
        sql.AppendLine("        , disease_name");
        sql.AppendLine("      order by");
        sql.AppendLine("        medical_history");
        sql.AppendLine("        , convert(date, regist_date) desc");
        sql.AppendLine("        , measure_date desc");
        sql.AppendLine("        , seq_no desc");
        sql.AppendLine("        , regist_date");
        sql.AppendLine("    ) uq");
        sql.AppendLine("  from");
        sql.AppendLine("  tbl_general_disease");
        sql.AppendLine("  where 1=1");
        sql.AppendLine("  and hosp_id = @hosp_id");
        sql.AppendLine("  and patient_id = @patient_id");
        sql.AppendLine("  and mask = @mask");
        sql.AppendLine("  and (medical_history in (");
        sql.Append(MedicalHistories.GetValues().Select(s => $"'{s.Replace("・未選択", "")}'").Aggregate((src, acc) => $"{src}, {acc}"));
        sql.AppendLine("  ) or medical_history is null) ");
        sql.AppendLine(") tgd");
        sql.AppendLine("where uq = 1");
        sql.AppendLine("order by");
        sql.AppendLine("sort_order");

        var param = new[] {
      createSqlParameter("@hosp_id", SqlDbType.VarChar, mySession.hosp_id),
      createSqlParameter("@patient_id", SqlDbType.VarChar, patientId),
      createSqlParameter("@mask", SqlDbType.SmallInt, 0),
    };

        var dt = new DataTable();
        sqlSelectTableNavi(sql.ToString(), param, ref dt);

        return dt;
    }

    public int GetExistsSentenceNaviDisease(string patientId)
    {
        var sql = new StringBuilder();
        sql.AppendLine("select");
        sql.AppendLine("count(*) cnt");
        sql.AppendLine("from");
        sql.AppendLine("tbl_general_disease");
        sql.AppendLine("where 1=1");
        sql.AppendLine("and hosp_id = @hosp_id");
        sql.AppendLine("and patient_id = @patient_id");
        sql.AppendLine("and mask = @mask");
        sql.AppendLine("and (medical_history is Null");
        sql.AppendLine("or medical_history in (");
        sql.Append(MedicalHistories.GetValues().Select(s => $"'{s.Replace("・未選択", "")}'").Aggregate((src, acc) => $"{src}, {acc}"));
        sql.AppendLine("))");

        var param = new[] {
          createSqlParameter("@hosp_id", SqlDbType.VarChar, mySession.hosp_id),
          createSqlParameter("@patient_id", SqlDbType.VarChar, patientId),
          createSqlParameter("@mask", SqlDbType.SmallInt, 0),
        };

        var dt = new DataTable();
        sqlSelectTableNavi(sql.ToString(), param, ref dt);

        return 0 < dt?.Rows?.Count ? Convert.ToInt32(dt.Rows[0][0]) : 0;
    }

    private void SetOptKindsNaviDisease()
    {
        var rdoli = Page.FindControl("rdoliOptKindsContainer") as RadioButtonList;
        foreach (var item in MedicalHistories.GetValues())
        {
            rdoli.Items.Add(item);
        }
        rdoli.SelectedIndex = 2;
    }

    /// <summary>
    /// 服用状況
    /// </summary>
    private static class MedicineHistories
    {
        public const string TakingMedicines = "服用中・未選択";
        public const string EndOfMedication = "服用終了";
        public const string InjectionInfusion = "注射・点滴";

        public static IReadOnlyList<string> GetValues()
        {
            return new[] {
        TakingMedicines,
        EndOfMedication,
        InjectionInfusion,
      };
        }
    }

    private DataTable GetSentenceNaviMedicine(string patientId)
    {
        var sql = new StringBuilder();
        sql.AppendLine("select");
        sql.AppendLine("case");
        sql.AppendLine("  when medical_history is null");
        sql.AppendLine("    then '服用中・未選択'");
        sql.AppendLine("  when medical_history = '服用中'");
        sql.AppendLine("    then '服用中・未選択' ");
        sql.AppendLine("  else medical_history");
        sql.AppendLine("  end kind");
        sql.AppendLine(", medicine_name sentence");
        sql.AppendLine(", '薬剤名' class2_name");
        sql.AppendLine("from");
        sql.AppendLine("(");
        sql.AppendLine("  select");
        sql.AppendLine("  medical_history");
        sql.AppendLine("  , medicine_name");
        sql.AppendLine("  , ROW_NUMBER() over(");
        sql.AppendLine("      order by");
        sql.AppendLine("        medical_history");
        sql.AppendLine("        , convert(date, regist_date) desc");
        sql.AppendLine("        , measure_date desc");
        sql.AppendLine("        , seq_no desc");
        sql.AppendLine("        , regist_date");
        sql.AppendLine("    ) sort_order");
        sql.AppendLine("  , ROW_NUMBER() over(");
        sql.AppendLine("      partition by");
        sql.AppendLine("        medical_history");
        sql.AppendLine("        , medicine_name");
        sql.AppendLine("      order by");
        sql.AppendLine("        medical_history");
        sql.AppendLine("        , convert(date, regist_date) desc");
        sql.AppendLine("        , measure_date desc");
        sql.AppendLine("        , seq_no desc");
        sql.AppendLine("        , regist_date");
        sql.AppendLine("    ) uq");
        sql.AppendLine("  from");
        sql.AppendLine("  tbl_taking_medicines");
        sql.AppendLine("  where 1=1");
        sql.AppendLine("  and hosp_id = @hosp_id");
        sql.AppendLine("  and patient_id = @patient_id");
        sql.AppendLine("  and mask = @mask");
        sql.AppendLine("  and (medical_history in (");
        sql.Append(MedicineHistories.GetValues().Select(s => $"'{s.Replace("・未選択", "")}'").Aggregate((src, acc) => $"{src}, {acc}"));
        sql.AppendLine("  ) or medical_history is null) ");
        sql.AppendLine(") ttm");
        sql.AppendLine("where uq = 1");
        sql.AppendLine("order by");
        sql.AppendLine("sort_order");

        var param = new[] {
          createSqlParameter("@hosp_id", SqlDbType.VarChar, mySession.hosp_id),
          createSqlParameter("@patient_id", SqlDbType.VarChar, patientId),
          createSqlParameter("@mask", SqlDbType.SmallInt, 0),
        };

        var dt = new DataTable();
        sqlSelectTableNavi(sql.ToString(), param, ref dt);

        return dt;
    }

    public int GetExistsSentenceNaviMedicine(string patientId)
    {
        var sql = new StringBuilder();
        sql.AppendLine("select");
        sql.AppendLine("count(*) cnt");
        sql.AppendLine("from");
        sql.AppendLine("tbl_taking_medicines");
        sql.AppendLine("where 1=1");
        sql.AppendLine("and hosp_id = @hosp_id");
        sql.AppendLine("and patient_id = @patient_id");
        sql.AppendLine("and mask = @mask");
        sql.AppendLine("and (medical_history is Null");
        sql.AppendLine("or medical_history in (");
        sql.Append(MedicineHistories.GetValues().Select(s => $"'{s.Replace("・未選択", "")}'").Aggregate((src, acc) => $"{src}, {acc}"));
        sql.AppendLine("))");

        var param = new[] {
          createSqlParameter("@hosp_id", SqlDbType.VarChar, mySession.hosp_id),
          createSqlParameter("@patient_id", SqlDbType.VarChar, patientId),
          createSqlParameter("@mask", SqlDbType.SmallInt, 0),
        };

        var dt = new DataTable();
        sqlSelectTableNavi(sql.ToString(), param, ref dt);

        return 0 < dt?.Rows?.Count ? Convert.ToInt32(dt.Rows[0][0]) : 0;
    }

    private void SetOptKindsNaviMedicine()
    {
        var rdoli = Page.FindControl("rdoliOptKindsContainer") as RadioButtonList;
        foreach (var item in MedicineHistories.GetValues())
        {
            rdoli.Items.Add(item);
        }
        rdoli.SelectedIndex = 0;
    }

    /// <summary>
    /// 定型文ボタン表示用
    /// </summary>
    /// <param name="tgt"></param>
    /// <param name="cnt"></param>
    public void setPnlSentenceBtns(string tgt, int cnt, string cc)
    {
        setPnlSentenceBtns(tgt, cnt, cc, String.Format("callSubWinSentence(this,'txt{0}','{1}')", tgt, cc));
    }

    private void setPnlSentenceBtns(string btnTgt, int cnt, string cc, string onClickAttrValue)
    {
        string[] cls = { "btn btnSizeSi", "btn btnSizeSi btnD" };
        Panel p = Page.FindControl("pnlBtn" + btnTgt) as Panel;
        if (p == null) return;
        p.CssClass = (cnt > 0) ? cls[0] : cls[1];
        if (cnt > 0) p.Attributes.Add("onclick", onClickAttrValue);
    }

    /// <summary>
    /// 定型文ボタン表示用
    /// </summary>
    public void setPnlSentenceBtnsNavi(string btnTgt, string txtTgt, int cnt, string cc, string patientId)
    {
        setPnlSentenceBtns(btnTgt, cnt, cc, String.Format("callSubWinSentence(this,'txt{0}','{1}', 'navi', '{2}')", txtTgt, cc, patientId));
    }

    public void rdoliOptKindsContainer_OnSelectedIndexChanged(object sender, EventArgs e)
    {
        DataPager p = Page.FindControl("dpSentence") as DataPager;
        NumericPagerField pagerField = p.Fields[1] as NumericPagerField;
        CommandEventArgs args = new CommandEventArgs("0", "");
        pagerField.HandleEvent(args);
    }

    /// <summary>
    /// ページャー用
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void dpSentence_PreRender(object sender, EventArgs e)
    {
        //jsl("[dpSentence_PreRender]");

        //ListView o = Page.FindControl("lvSentence") as ListView;
        //DataPager p = Page.FindControl("dpSentence") as DataPager;
        //if (o == null || p == null || ViewState["dtSentence"] == null) return;

        //DataTable dt = ViewState["dtSentence"] as DataTable;
        //o.DataSource = dt;
        //o.DataBind();
        //p.SetPageProperties(0, p.MaximumRows, true);

        //Panel pnl = Page.FindControl("pnlDpSentence") as Panel;
        //if (pnl != null) pnl.CssClass = (dt.Rows.Count > (p.PageSize + 1)) ? "" : "dn";

        ListView o = Page.FindControl("lvSentence") as ListView;
        DataPager p = Page.FindControl("dpSentence") as DataPager;

        //前頁
        NextPreviousPagerField pagerFieldP = new NextPreviousPagerField();
        //次頁
        NextPreviousPagerField pagerFieldN = new NextPreviousPagerField();

        //数字
        NumericPagerField pagerField = new NumericPagerField();

        //0件チェック・ページャーボタンの表示・非表示セット
        DataTable dtListView = (DataTable)ViewState["dtSentence"];

        var dv = new DataView(dtListView);

        if (dv != null) //if (dtListView != null)
        {
            var rdoli = Page.FindControl("rdoliOptKindsContainer") as RadioButtonList;
            if (rdoli != null && 0 < rdoli.Items.Count)
            {
                //jsl("[dpSentence_PreRender]", "SelectedValue:", rdoli.SelectedValue);
                dv.RowFilter = $"kind = '{rdoli.SelectedValue}'";
            }

            if (dv.Count < 1) //if (dtListView.Rows.Count < 1)
            {
                showPager(false, ref pagerFieldP, ref pagerField, ref pagerFieldN);
            }
            else
            {
                showPager(true, ref pagerFieldP, ref pagerField, ref pagerFieldN);
            }
        }

        //一旦消して、追加する。
        //p.Fields.Clear();
        //p.Fields.Add(pagerFieldP);
        //p.Fields.Add(pagerField);
        //p.Fields.Add(pagerFieldN);
        o.DataSource = dv; //dtListView;
        o.DataBind();
        js("setSentenceMax()");
        js("fncPageboxS()");
    }

    #endregion 定型文

    /// <summary>
    /// チェックボックス保存用の値を取得
    /// </summary>
    /// <param name="o"></param>
    /// <returns>true:1,false:0</returns>
    public string getChk(CheckBox o)
    {
        return (o.Checked == true) ? "1" : "0";
    }



    /// <summary>
    /// セッションチェック
    /// </summary>
    /// <param name="n"></param>
    public void checkSession(int n)
    {
        checkSession(n, true);
    }

    /// <summary>
    /// セッションチェック
    /// </summary>
    /// <param name="n"></param>
    /// <param name="updateFlg"></param>
    public void checkSession(int n, bool updateFlg)
    {
        int rtn;
        rtn = updateSessionTime(n, updateFlg);

        //if (mySession.login_id.Length > 0 && rtn != 100) return;
        if (rtn != 100) return;

        bool mediadmin = (getSession("MEDIA_USER") == "1") ? true :
                        (getSession("MEDIA_ADMIN") == "1") ? true :
                        false;

        string s = (n == 1) ? mediadmin ? "media/Mgt/mglogin" : "Mgt/glogin" :
                   (n == 2) ? "Mgt/slogin" :
                   mediadmin ? "media/mlogin" : "login";

        logLong(2, cmnName, Request.Path.TrimStart(new char[] { '/' }), MethodBase.GetCurrentMethod().Name + "ログイン情報無し：loginページに移動 ", "", "", "");
        doRedirect(string.Format("~/{0}.aspx?loginFlg=1", s));
    }


    /// <summary>
    /// tbl_session 更新
    /// </summary>
    /// <param name="n"></param>
    /// <param name="updateFlg">false：updateしないで削除だけ</param>
    /// <returns></returns>
    private int updateSessionTime(int n, bool updateFlg)
    {
        try
        {
            bool sqlrtn = false;
            var sqlString = new StringBuilder();

            if (updateFlg == true)
            {
                sqlString.AppendLine("update tbl_session ");
                sqlString.AppendLine("set regist_datetime = getdate() ");
                sqlString.AppendLine("where ");
                sqlString.AppendLine(" group_id = @group_id");
                sqlString.AppendLine(" and user_id = @user_id");
                sqlString.AppendLine(" and session_flg = @session_flg");
                sqlString.AppendLine(" and regist_datetime >= DATEADD(MI, -60, getdate())");

                var param = new SqlParameter[]
                {
                    createSqlParameter("@group_id", SqlDbType.VarChar, sqlTabooChar(mySession.group_id)),
                    createSqlParameter("@user_id", SqlDbType.VarChar, sqlTabooChar(mySession.login_id)),
                    createSqlParameter("@session_flg", SqlDbType.VarChar, sqlTabooChar(n.ToString())),
                };

                var p = new string[] { sqlString.ToString() };
                var lstparam = new SqlParameter[][] { param }.ToList();
                sqlrtn = sqlTran(p, lstparam);
                if (!sqlrtn)
                {
                    //js("alertmsg('エラー', 'セッション情報登録エラー。', 'ＯＫ', \"alertclose('');\")");
                    logLong(4, cmnName, "updateSessionTime", "tbl_session update error", "", "", "");
                    return -1;
                }
                sqlString.Clear();
            }

            sqlString.AppendLine("delete tbl_session ");
            sqlString.AppendLine(" where regist_datetime < DATEADD(MI, -60, getdate()) ");
            sqlrtn = sqlTran(sqlString.ToString());
            if (!sqlrtn)
            {
                //js("alertmsg('エラー', 'セッション情報登録エラー。', 'ＯＫ', \"alertclose('');\")");
                logLong(4, cmnName, "deleteSessionTime", "tbl_session delete error", "", "", "");
                return -1;
            }

            if (n != 2)
            {

                DataTable dt = new DataTable();
                sqlString.Clear();
                sqlString.AppendLine("select * from tbl_session ");
                sqlString.AppendLine("where ");
                sqlString.AppendLine(" group_id = @group_id");
                sqlString.AppendLine(" and user_id = @user_id");
                sqlString.AppendLine(" and session_flg = @session_flg");

                SqlParameter[] param1 = {
                  createSqlParameter("@group_id", SqlDbType.VarChar, sqlTabooChar(mySession.group_id)),
                  createSqlParameter("@user_id", SqlDbType.VarChar, sqlTabooChar(mySession.login_id)),
                  createSqlParameter("@session_flg", SqlDbType.VarChar, sqlTabooChar(n.ToString())),
                };
                sqlrtn = sqlSelectTable(sqlString.ToString(), param1, ref dt);
                if (!sqlrtn)
                {
                    logLong(4, cmnName, "selectSession", "tbl_session select error", "", "", "");
                    return -1;
                }

                if (dt.Rows.Count == 0)
                {
                    return 100;
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "updateSessionTime", ex.Message, "", "", "");
            return -1;
        }

    }

    /// <summary>
    /// 介護度マスタDT取得
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public DataTable getCareLevelDT(string d)
    {
        DataTable dt = new DataTable();
        string sql = "select level_code, level_name";
        sql += " from mst_care_level";
        sql += " where '" + sqlTabooChar(d) + "' between st_date and ed_date";
        sql += " order by index_no";
        bool sqlrtn = sqlSelectTable(sql, ref dt);
        if (!sqlrtn) logLong(4, cmnName, Request.Path.TrimStart(new char[] { '/' }), MethodBase.GetCurrentMethod().Name + "介護度取得エラー:" + sql, "", "", "");
        return dt;
    }


    /// <summary>
    /// ディレクトリ作成
    /// </summary>
    /// <param name="path">作成するディレクトリ</param>
    /// <returns>成否</returns>
    public bool createDir(string path)
    {
        bool bl = true;
        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                bl = false;
                logLong(4, cmnName, Request.Path.TrimStart(new char[] { '/' }), string.Format("ディレクトリ作成エラー:{0}:{1}", path, e.ToString()), "", "", "");
            }
        }
        return bl;
    }

    /// <summary>
    /// DrとDHのデータテーブル取得
    /// </summary>
    /// <param name="n">3:Dr 4:DH</param>
    /// <returns></returns>
    public DataTable getDdlDrDH(int n)
    {
        DataTable dt = new DataTable();
        string sql = "";
        sql = "select localuser_id, localuser_scrn, localuser_name from mst_local_user";
        sql += " where hosp_id = '" + sqlTabooChar(mySession.hosp_id) + "'";
        sql += " and job_id = '" + n.ToString() + "'";
        sql += " and mask = '0'";
        sql += " order by index_no";
        bool sqlrtn = sqlSelectTable(sql, ref dt);
        if (!sqlrtn) logLong(4, "", "", "Dr/DH取得エラー：" + sqlTabooChar(sql), "", "", "");
        return dt;
    }

    /// <summary>
    /// 指定した文字列が数値であれば true。それ以外は false。
    /// </summary>
    /// <param name="stTarget"></param>
    /// <returns></returns>
    public bool IsNumeric(string stTarget)
    {
        double dNullable;

        return double.TryParse(
            stTarget,
            System.Globalization.NumberStyles.Any,
            null,
            out dNullable
        );
    }

    /// <summary>
    /// ページャの表示・非表示
    /// </summary>
    /// <returns></returns>
    public bool showPager(bool show, ref NextPreviousPagerField pagerFieldP, ref NumericPagerField pagerField, ref NextPreviousPagerField pagerFieldN)
    {

        try
        {
            //前頁
            //NextPreviousPagerField pagerFieldP = new NextPreviousPagerField();
            pagerFieldP.ShowFirstPageButton = show;
            pagerFieldP.ShowPreviousPageButton = show;
            pagerFieldP.ShowNextPageButton = false;
            pagerFieldP.FirstPageText = "<<";
            pagerFieldP.PreviousPageText = "<";
            pagerFieldP.ButtonCssClass = "pagerFp";
            pagerFieldP.RenderNonBreakingSpacesBetweenControls = false;

            //次頁
            //NextPreviousPagerField pagerFieldN = new NextPreviousPagerField();
            pagerFieldN.ShowNextPageButton = show;
            pagerFieldN.ShowLastPageButton = show;
            pagerFieldN.ShowPreviousPageButton = false;
            pagerFieldN.NextPageText = ">";
            pagerFieldN.LastPageText = ">>";
            pagerFieldN.ButtonCssClass = "pagerFn";
            pagerFieldN.RenderNonBreakingSpacesBetweenControls = false;

            //数字
            //NumericPagerField pagerField = new NumericPagerField();
            pagerField.ButtonCount = 3;
            pagerField.ButtonType = ButtonType.Link;
            pagerField.CurrentPageLabelCssClass = "pagerD";
            pagerField.NumericButtonCssClass = "pagerD";
            pagerField.PreviousPageText = "…";
            pagerField.NextPageText = "…";
            pagerField.RenderNonBreakingSpacesBetweenControls = false;
            pagerField.NextPreviousButtonCssClass = "pgDn";

            return true;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "exclusive", "排他チェックエラー:" + ex.Message, "", "", "");
            return false;
        }

    }

    /// <summary>
    /// ファイル更新日を取得
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public string getUpdateTime(string s)
    {
        DateTime dtUpdate = System.IO.File.GetLastWriteTime(@s);
        return dtUpdate.ToString().Replace(" ", "_").Replace("/", "_").Replace(":", "_");
    }



    /// <summary>
    /// 文字列を暗号化する
    /// </summary>
    /// <param name="sourceString">暗号化する文字列</param>  
    /// <returns>暗号化された文字列</returns>
    public static string getEncryptString(string sourceString, string groupId, string userId)
    {
        //RijndaelManagedオブジェクトを作成
        System.Security.Cryptography.RijndaelManaged rijndael =
            new System.Security.Cryptography.RijndaelManaged();

        //パスワードから共有キーと初期化ベクタを作成
        byte[] key, iv;
        generateKeyFromPassword(
            getTmpPassword(groupId, userId), getTmpSalt(groupId, userId), rijndael.KeySize, out key, rijndael.BlockSize, out iv);
        rijndael.Key = key;
        rijndael.IV = iv;

        //文字列をバイト型配列に変換する
        byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(sourceString);

        //対称暗号化オブジェクトの作成
        System.Security.Cryptography.ICryptoTransform encryptor =
            rijndael.CreateEncryptor();
        //バイト型配列を暗号化する
        byte[] encBytes = encryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
        //閉じる
        encryptor.Dispose();

        //バイト型配列を文字列に変換して返す
        return System.Convert.ToBase64String(encBytes);
    }

    /// <summary>
    /// 暗号化された文字列を復号化する
    /// </summary>
    /// <param name="sourceString">暗号化された文字列</param>
    /// <param name="password">暗号化に使用したパスワード</param>
    /// <returns>復号化された文字列</returns>
    public string getDecryptString(string sourceString, string groupId, string userId)
    {
        string rtn = "";
        try
        {
            //RijndaelManagedオブジェクトを作成
            System.Security.Cryptography.RijndaelManaged rijndael =
              new System.Security.Cryptography.RijndaelManaged();

            //パスワードから共有キーと初期化ベクタを作成
            byte[] key, iv;
            generateKeyFromPassword(
                getTmpPassword(groupId, userId), getTmpSalt(groupId, userId), rijndael.KeySize, out key, rijndael.BlockSize, out iv);
            rijndael.Key = key;
            rijndael.IV = iv;

            //文字列をバイト型配列に戻す
            byte[] strBytes = System.Convert.FromBase64String(sourceString);

            //対称暗号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform decryptor =
                rijndael.CreateDecryptor();
            //バイト型配列を復号化する
            //復号化に失敗すると例外CryptographicExceptionが発生
            byte[] decBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);

            //閉じる
            decryptor.Dispose();

            //バイト型配列を文字列に戻して返す
            rtn = System.Text.Encoding.UTF8.GetString(decBytes);

        }
        catch (Exception ex)
        {
            logLong(4, cmnName, "exclusive", "排他チェックエラー:" + ex.Message, "", "", "");
            rtn = "複合化に失敗しました";
        }

        return rtn;
    }

    /// <summary>
    /// パスワードから共有キーと初期化ベクタを生成する
    /// </summary>
    /// <param name="password">基になるパスワード</param>
    /// <param name="keySize">共有キーのサイズ（ビット）</param>
    /// <param name="key">作成された共有キー</param>
    /// <param name="blockSize">初期化ベクタのサイズ（ビット）</param>
    /// <param name="iv">作成された初期化ベクタ</param>
    private static void generateKeyFromPassword(string password, string saltStr,
        int keySize, out byte[] key, int blockSize, out byte[] iv)
    {
        //パスワードから共有キーと初期化ベクタを作成する
        //saltを決める
        byte[] salt = System.Text.Encoding.UTF8.GetBytes(saltStr);
        //Rfc2898DeriveBytesオブジェクトを作成する
        System.Security.Cryptography.Rfc2898DeriveBytes deriveBytes =
            new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt);
        //反復処理回数を指定する デフォルトで1000回
        deriveBytes.IterationCount = 1000;
        //共有キーと初期化ベクタを生成する
        key = deriveBytes.GetBytes(keySize / 8);
        iv = deriveBytes.GetBytes(blockSize / 8);
    }

    private static string getTmpPassword(string groupId, string userId)
    {
        return string.Format("with{0}you{1}", groupId, userId);
    }
    private static string getTmpSalt(string groupId, string userId)
    {
        return string.Format("with{1}you{0}media", groupId, userId);
    }


    /// <summary>
    /// tbl_sessionのuser_nameを取得する
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="groupId"></param>
    /// <param name="hospId"></param>
    /// <returns></returns>
    public string getTblSessionUserName(string userId, string groupId, string hospId)
    {

        string sql = "select user_name from tbl_session"
        + " where user_id=@USERID"
        + " and group_id=@GROUPID"
        + " and hosp_id=@HOSPID"
        ;

        SqlParameter[] param = new SqlParameter[3];
        param[0] = createSqlParameter("@USERID", SqlDbType.VarChar, sqlTabooChar(userId));
        param[1] = createSqlParameter("@GROUPID", SqlDbType.VarChar, sqlTabooChar(groupId));
        param[2] = createSqlParameter("@HOSPID", SqlDbType.VarChar, sqlTabooChar(hospId));

        return sqlSelect(sql, "user_name", param);
    }

    /// <summary>
    /// tbl_sessionのuser_nameを更新する
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="groupId"></param>
    /// <param name="hospId"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public bool updateTblSessionUserName(string userId, string groupId, string hospId, string userName, int sessionFlg)
    {

        string sql = "update tbl_session"
        + " set user_name=@USERNAME"
        + string.Format(",session_flg='{0}'", sessionFlg.ToString())
        + " where user_id=@USERID"
        + " and group_id=@GROUPID"
        + " and hosp_id=@HOSPID"
        ;

        SqlParameter[] param = new SqlParameter[4];
        param[0] = createSqlParameter("@USERNAME", SqlDbType.VarChar, sqlTabooChar(userName));
        param[1] = createSqlParameter("@USERID", SqlDbType.VarChar, sqlTabooChar(userId));
        param[2] = createSqlParameter("@GROUPID", SqlDbType.VarChar, sqlTabooChar(groupId));
        param[3] = createSqlParameter("@HOSPID", SqlDbType.VarChar, sqlTabooChar(hospId));

        return sqlUpdate(sql, param);
    }

    /// <summary>
    /// SHA256でハッシュ化する
    /// </summary>
    /// <param name="pwd"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    public static string getHashPassword(string pwd, string salt)
    {
        var result = "";
        var saltAndPwd = String.Concat(pwd, salt);
        var encoder = new System.Text.UTF8Encoding();
        var buffer = encoder.GetBytes(saltAndPwd);
        using (var csp = new System.Security.Cryptography.SHA256CryptoServiceProvider())
        {
            var hash = csp.ComputeHash(buffer);
            result = Convert.ToBase64String(hash);
        }
        return result;
    }

    /// <summary>
    /// ブラウザ判定
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private void Page_PreLoad(object sender, EventArgs e)
    {
        AppendHeaderCSP();
        //2018/04/26 iPadのSafariで使えるように変更
        //2020/01/22 iPad OS で使えるように変更（サーバ側では Macintosh と区別がつかないので、Macintosh は JavaScript で遷移させるように変更した）
        string userAgent = Request.UserAgent.ToLower();
        if (userAgent.IndexOf("googlebot") >= 0
           || userAgent.IndexOf("opera") >= 0 || userAgent.IndexOf("opr") >= 0 || userAgent.IndexOf("edge") >= 0
           || (userAgent.IndexOf("safari") >= 0 && userAgent.IndexOf("ipad") >= 0 && userAgent.IndexOf("crios") >= 0))
        {
            doRedirect("~/browserErr.aspx");
        }
    }

    //iPad用 ReadOnly
    protected string getIpadReadOnly()
    {
        return isiPad() ? " readonly='readonly' " : "";
    }

    /// <summary>
    /// 訪補助表示切り替え届出確認
    /// </summary>
    /// <returns>true：該当する。／false：該当しない。</returns>
    protected bool getTodokedeSupportDH(string hospId, string date)
    {
        var ss = new[] { "7", "2", "6", };

        /*
          1	在宅歯科医療推進加算
          2	在宅療養支援歯科診療所 → (2018/4/1～)在宅療養支援歯科診療所2
          3	地域医療連携体制加算
          4	在宅患者治療総合医療管理料
          5	かかりつけ歯科医機能強化型歯科診療所
          6	歯科訪問診療料の注１３の届出

          ■追加(2018/4/1～)
          7	在宅療養支援歯科診療所1
          8	院内感染防止
        */

        var sql = new StringBuilder();
        sql.AppendLine("select");

        var sep = "";
        for (var i = 1; i < ss.Length + 1; i++)
        {
            //k99.cnt99
            sql.AppendFormat("{0}k{1}.cnt{1}", sep, i).AppendLine();
            sep = ",";
        }

        sql.AppendLine("from");

        var where = new StringBuilder();
        where.AppendLine("where hosp_id = @HOSP_ID");
        where.AppendLine("and start_date <= @DATE and @DATE <= end_date");

        var param = new[] {
      createSqlParameter("@HOSP_ID", SqlDbType.VarChar, sqlTabooChar(hospId)),
      createSqlParameter("@DATE", SqlDbType.VarChar, sqlTabooChar(date))
    };

        sep = "";
        for (var i = 1; i < ss.Length + 1; i++)
        {
            sql.AppendFormat("{0}(select count(*) as cnt{1} from mst_kasan", sep, i).AppendLine();
            sql.AppendLine(where.ToString());
            sql.AppendFormat("and kasan_code = '{0}') k{1}", ss[i - 1], i).AppendLine();
            sep = ",";
        }

        //jsl(sql.ToString());

        var dt = new DataTable();
        sqlSelectTable(sql.ToString(), param, ref dt);

        var res = false;
        if (dt.Rows.Count > 0)
        {
            for (var i = 1; i < ss.Length + 1; i++)
            {
                var cnt = Convert.ToInt32(dt.Rows[0]["cnt" + i.ToString()]);
                //jsl(string.Join(" ", "kasan_code:", ss[i - 1], "cnt:", cnt.ToString()));
                if (0 < cnt)
                {
                    res = true;
                    break;
                }
            }
        }
        return res;
    }

    /// <summary>
    /// 訪補助表示切り替え届出確認
    /// </summary>
    /// <returns>true：該当する。／false：該当しない。</returns>
    protected bool getTodokedeSupportDH(string date)
    {
        return getTodokedeSupportDH(mySession.hosp_id, date);
    }

    /// <summary>
    /// 元号情報読込
    /// </summary>
    /// <returns>mst_wagou情報をlocalstorageへ読み込みます。</returns>
    protected bool loadGengo()
    {
        try
        {
            var sql = "SELECT * FROM mst_wagou ORDER BY year_from, month_from, day_from";
            var dtWareki = new DataTable();
            sqlSelectTable(sql, ref dtWareki);

            var localStoragevalue = new StringBuilder();

            localStoragevalue.Append("[");

            foreach (DataRow row in dtWareki.Rows)
            {
                var localStorageitem = new StringBuilder();

                localStorageitem.Append("{");

                //wagou_code varchar(2)
                localStorageitem.AppendFormat("wagou_code:'{0}',", row["wagou_code"]);

                //ad_n8_from
                localStorageitem.AppendFormat("ad_n8_from:{0:0000}{1:00}{2:00},"
                  , row["year_from"]
                  , row["month_from"]
                  , row["day_from"]
                  );

                //ad_n8_to
                localStorageitem.AppendFormat("ad_n8_to:{0:0000}{1:00}{2:00},"
                  , row["year_to"]
                  , row["month_to"]
                  , row["day_to"]
                  );

                //index_no smallint
                localStorageitem.AppendFormat("index_no:{0},", row["index_no"]);

                //wagou_name varchar(10)
                localStorageitem.AppendFormat("wagou_name:'{0}',", row["wagou_name"]);

                //wagou_name_abbr
                localStorageitem.AppendFormat("wagou_name_abbr:'{0}',", row["wagou_name"].ToString().Substring(0, 1));

                //year_from smallint
                localStorageitem.AppendFormat("year_from:{0},", row["year_from"]);

                //month_from smallint
                localStorageitem.AppendFormat("month_from:{0},", row["month_from"]);

                //day_from smallint
                localStorageitem.AppendFormat("day_from:{0},", row["day_from"]);

                //year_to smallint
                localStorageitem.AppendFormat("year_to:{0},", row["year_to"]);

                //month_to smallint
                localStorageitem.AppendFormat("month_to:{0},", row["month_to"]);

                //day_to smallint
                localStorageitem.AppendFormat("day_to:{0},", row["day_to"]);

                localStorageitem.Append("},");
                //jsl(localStorageitem.ToString());

                localStoragevalue.Append(localStorageitem.ToString());
            }

            localStoragevalue.Append("]");
            //jsl(localStoragevalue.ToString());

            js(string.Format("localStorage.setItem('GengoTbl', JSON.stringify({0}));", localStoragevalue.ToString()));

            return true;
        }
        catch (Exception ex)
        {
            logLong(4, cmnName, MethodBase.GetCurrentMethod().Name, ex.Message, "", "", "");
            return false;
        }
    }

    public string getManualUrl()
    {
        return getConst("ManualUrl");
        //string url = "";

        //DataTable dt = new DataTable();

        //var sqlString = new StringBuilder();
        //sqlString.Clear();
        //sqlString.AppendLine("select * from mst_options ");
        //sqlString.AppendLine("where ");
        //sqlString.AppendLine(" option_id = '1'");

        //bool sqlrtn = false;
        //sqlrtn = sqlSelectTable(sqlString.ToString(), ref dt);

        //if(dt.Rows.Count > 0)
        //{
        //  url = dt.Rows[0]["option_url"].ToString();
        //}
        //return url;
    }

    public string getDrugInfoUrl()
    {
        return getConst("DrugInfoUrl");
        //string url = "";

        //DataTable dt = new DataTable();

        //var sqlString = new StringBuilder();
        //sqlString.Clear();
        //sqlString.AppendLine("select * from mst_options ");
        //sqlString.AppendLine("where ");
        //sqlString.AppendLine(" option_id = '2'");

        //bool sqlrtn = false;
        //sqlrtn = sqlSelectTable(sqlString.ToString(), ref dt);

        //if (dt.Rows.Count > 0)
        //{
        //  url = dt.Rows[0]["option_url"].ToString();
        //}
        //return url;
    }

    public string getProfileUrl()
    {
        return getConst("ProfileUrl");
        //string url = "";

        //DataTable dt = new DataTable();

        //var sqlString = new StringBuilder();
        //sqlString.Clear();
        //sqlString.AppendLine("select * from mst_options ");
        //sqlString.AppendLine("where ");
        //sqlString.AppendLine(" option_id = '3'");

        //bool sqlrtn = false;
        //sqlrtn = sqlSelectTable(sqlString.ToString(), ref dt);

        //if (dt.Rows.Count > 0)
        //{
        //  url = dt.Rows[0]["option_url"].ToString();
        //}
        //return url;
    }

    /// <summary>
    /// Content-Security-Policy
    /// </summary>
    protected void AppendHeaderCSP()
    {
        var value = new StringBuilder();

        //CSP: default-src
        value.Append(string.Join(" ", new[] {
      "default-src",
      "'self'",
    })).Append(";");

        //CSP: style-src
        value.Append(string.Join(" ", new[] {
      "style-src",
      "'self'",
      "'unsafe-inline'",
    })).Append(";");

        //CSP: script-src
        value.Append(string.Join(" ", new[] {
      "script-src",
      "'self'",
      "'unsafe-inline'",
      "'unsafe-eval'",
      "https://cdnjs.cloudflare.com",
      "https://ajax.googleapis.com",
    })).Append(";");

        //CSP: frame-src
        value.Append(string.Join(" ", new[] {
      "frame-src",
      "'self'",
      PostMessageSettings.MediaApps.Origin,
    })).Append(";");

        //CSP: frame-ancestors
        value.Append(string.Join(" ", new[] {
      "frame-ancestors",
      "'self'",
      PostMessageSettings.MediaApps.Origin,
    })).Append(";");

        //CSP: img-src 
        value.Append(string.Join(" ", new[] {
      "img-src ",
      "'self'",
      getConst("endPoint"),
    })).Append(";");

        //Response.AppendHeader("Content-Security-Policy-Report-Only", value.ToString());
        Response.AppendHeader("Content-Security-Policy", value.ToString());
    }

    public string getUniqueId(string patientId, string hospId)
    {
        string sql = "select unique_id from tbl_patient where patient_id = @PATIENT_ID and hosp_id = @HOSP_ID";
        SqlParameter[] param = {
                  createSqlParameter("@PATIENT_ID", SqlDbType.VarChar, patientId),
                  createSqlParameter("@HOSP_ID", SqlDbType.VarChar, hospId)
                };
        string uniqueId = sqlSelect(sql, "unique_id", param);
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = "uniqueId";
        }
        return uniqueId;
    }

    public string getInsertTblDataSetFlgSQL(string patientId,out string filePath){
        string hospId = getSession("HOSP_ID"), userId = getSession("LOGIN_ID"),
               uid = $"{hospId}_{userId}_{patientId}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        filePath = $@"{getConst("filePath")}send/{uid}";
        return "insert into tbl_data_set_flg " +
               "(hosp_id, user_id, patient_id, msg_uid, file_path, set_status,set_time,data_type)" +
               $" values ('{sqlTabooChar(hospId)}', '{sqlTabooChar(userId)}', '{sqlTabooChar(patientId)}'," +
               $"'{sqlTabooChar(uid)}', '{sqlTabooChar(filePath)}', '1', SYSDATETIME() ,'1')";
    }

    public string getInsertTblDataSetFlgKInputListSQL(string yyyyMMdd)
    {
        string hospId = getSession("HOSP_ID"), userId = getSession("LOGIN_ID"), patientId = "KINPUTLST",
               uid = $"{hospId}_{userId}_{patientId}_{DateTime.Now.ToString("yyyyMMddHHmmss")}",
               file_path = Path.Combine(getConst("filePath"), patientId, hospId, yyyyMMdd).Replace("\\", "/");
        return "insert into tbl_data_set_flg " +
               "(hosp_id, user_id, patient_id, msg_uid, file_path, set_status,set_time,data_type)" +
               $" values ('{sqlTabooChar(hospId)}', '{sqlTabooChar(userId)}', '{sqlTabooChar(patientId)}'," +
               $" '{sqlTabooChar(uid)}', '{sqlTabooChar(file_path)}', '1', SYSDATETIME() ,'2')";
    }

    public bool setKInputList(string yyyyMMdd)
    {
        return sqlInsert(getInsertTblDataSetFlgKInputListSQL(yyyyMMdd));
    }

}
