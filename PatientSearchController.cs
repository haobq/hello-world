using System;
using System.Collections.Generic;
using System.Web.Mvc;
using PFDb;
using profile.Controllers.Common;
using profile.Models;
using profile.Common;
using profile.Models.Common;
using profile.Models.Service;
using static profile.Models.PatientSearchViewModel;


namespace profile.Controllers
{
    public class PatientSearchController : AppController
    {
        // GET: PatientSearch
        public ActionResult Index()
        {
            return View("PatientSearch");
        }
        public ActionResult PatientSearch()
        {
            return Index();
        }

        /// <summary>
        /// 患者listを取得
        /// </summary>
        /// <param name="txtVisitDateInc">訪問日</param>
        /// <param name="ddlDrInc">DR/DH</param>
        /// <param name="ddlKbn">訪問先</param>
        /// <param name="txtVisit">施設名</param>
        /// <param name="txtPatientNameInc">患者名</param>
        /// <param name="txtPatientIdInc">患者番号</param>
        /// <param name="sort">表示順</param>
        /// <param name="flag">0:初期化時、セッションの検索条件を使う; 1:検索ボタン押下ま、今の検索条件を使う 2:表示順を選択時、セッションの検索条件を使う;</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        //public ActionResult GetPatientSearchList(string txtVisitDateInc, string ddlDrInc, string ddlKbn, string ddlVisit, string txtPatientNameInc, string txtPatientIdInc, string sort, string flag, string sortFlag)
         public ActionResult GetPatientSearchList(string txtVisitDateInc, string ddlDrInc, string ddlKbn, string txtVisit, string txtPatientNameInc, string txtPatientIdInc, string sort, string flag, string sortFlag)
        {
            if (!Request.IsAjaxRequest())
            {
                return PartialView("_PatientSearchResult");
            }
            PatientSearchViewModel patientSearchViewModel = new PatientSearchViewModel();
            PatientSearchService service = new PatientSearchService();
            inputModel model = new inputModel();

            //訪問日が日付型で無ければ、無視する。（空白として扱う）
            if (!DateTime.TryParse(txtVisitDateInc, out DateTime _)) txtVisitDateInc = "";

            if (flag == "1")
            {
                model.txtVisitDateInc = txtVisitDateInc;
                model.ddlDrInc = ddlDrInc;
                model.ddlKbn = ddlKbn;
                //model.ddlVisit = txtVisit;
                model.txtVisit = txtVisit;
                model.txtPatientNameInc = txtPatientNameInc;
                model.txtPatientIdInc = txtPatientIdInc;
                model.sortStr = sort;
                model.sortFlag = sortFlag;
            }
            else
            {
                if (mySession.search_visit_date != "" || mySession.search_dr != "" || mySession.search_kbn != "" ||
                    mySession.search_visit != "" || mySession.search_patient_name != "" || mySession.search_patient_id != "" || mySession.patient_search_sort != "")
                {
                    model.txtVisitDateInc = mySession.search_visit_date.ToString();
                    model.ddlDrInc = mySession.search_dr.ToString();
                    model.ddlKbn = mySession.search_kbn.ToString();
                    //model.ddlVisit = mySession.search_visit.ToString();
                    model.txtVisit = mySession.search_visit.ToString();
                    model.txtPatientNameInc = mySession.search_patient_name.ToString();
                    model.txtPatientIdInc = mySession.search_patient_id.ToString();
                    model.sortStr = flag == "0" ? mySession.patient_search_sort.ToString() : sort;
                    model.sortFlag = flag == "0" ? mySession.patient_search_sort_flag.ToString() : sortFlag;
                }
                else if(flag == "0")
                {
                    return PartialView("_PatientSearchResult", patientSearchViewModel);
                }
                else if (flag == "2")
                {
                    model.sortStr = sort;
                }
            }

            List<PatientSearchResultModel> resultList = service.GetPatientSearchList(model);
            //セッションに検索条件を保存する
            service.RegistSession(model);
            patientSearchViewModel.ResultList = resultList;
            return PartialView("_PatientSearchResult", patientSearchViewModel);
        }

        /// <summary>
        /// DR/DHlist、訪問先list、施設名listを取得
        /// </summary>
        /// <param name="model">検索条件</param>
        /// <param name="txtVisitDateInc">訪問日</param>
        /// <param name="flag">0:訪問日変更の場合; 1:訪問先変更の場合; 2:初期化の場合</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult GetSearchCondition(inputModel model, String txtVisitDateInc, String flag)
        {
            if (!Request.IsAjaxRequest())
            {
                return PartialView("_PatientSearchResult");
            }
            PatientSearchService service = new PatientSearchService();
            PatientSearchViewModel patientSearchViewModel = new PatientSearchViewModel();

            //訪問日が日付型で無ければ、無視する。（空白として扱う）
            if (!DateTime.TryParse(txtVisitDateInc, out DateTime _)) txtVisitDateInc = "";

            if (flag == "0")
            {
                patientSearchViewModel.ddlDrInc = "";
                patientSearchViewModel.ddlKbn = "";
            }
            else if (flag == "1")
            {
                patientSearchViewModel.ddlDrInc = model.ddlDrInc;
                patientSearchViewModel.ddlKbn = model.ddlKbn;
            }
            else if (flag == "2")
            {
                patientSearchViewModel.ddlDrInc = mySession.search_dr ?? "";
                patientSearchViewModel.ddlKbn = mySession.search_kbn ?? "";
                patientSearchViewModel.ddlVisit = mySession.search_visit ?? "";
            }
            patientSearchViewModel.doctortList = service.GetDoctorList(txtVisitDateInc);
            patientSearchViewModel.visitList = service.GetVisitList(txtVisitDateInc);
            patientSearchViewModel.visitNameList = service.GetVisitNameList(txtVisitDateInc, model.ddlKbn);
            return PartialView("_PatientSearchInput", patientSearchViewModel);
        }
    }
}
