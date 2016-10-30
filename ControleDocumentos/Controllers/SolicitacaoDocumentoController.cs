﻿using ControleDocumentos.Repository;
using ControleDocumentos.Util.Extension;
using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentos.Util;

namespace ControleDocumentos.Controllers
{
    public class SolicitacaoDocumentoController : Controller
    {
        TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();
        CursoRepository cursoRepository = new CursoRepository();
        AlunoRepository alunoRepository = new AlunoRepository();
        SolicitacaoDocumentoRepository solicitacaoRepository = new SolicitacaoDocumentoRepository();
        DocumentoRepository documentoRepository = new DocumentoRepository();

        // GET: SolicitacaoDocumento
        public ActionResult Index()
        {
            PopularDropDowns();

            return View(solicitacaoRepository.GetAll());
        }

        public ActionResult CadastrarSolicitacao(int? idSol)
        {
            PopularDropDownsCadastro();
            SolicitacaoDocumento sol = new SolicitacaoDocumento();

            if (idSol.HasValue)
            {
                sol = solicitacaoRepository.GetSolicitacaoById((int)idSol);
                PopularDropDownAlunos(sol.AlunoCurso.Curso.IdCurso);
            }
            else
            {
                ViewBag.Alunos = new SelectList(new List<SelectListItem>() { new SelectListItem() {
                    Text ="Selecione um curso",
                    Value =""}
                }, "Value", "Text");
            }
            
            //retorna model
            return PartialView("_CadastroSolicitacao", sol);
        }

        public ActionResult List(Models.SolicitacaoDocumentoFilter filter)
        {
            return PartialView("_List", solicitacaoRepository.GetByFilter(filter));
        }

        public ActionResult CarregaModalExclusao(int idSol)
        {
            SolicitacaoDocumento sol = solicitacaoRepository.GetSolicitacaoById(idSol);
            return PartialView("_ExclusaoSolicitacao", sol);
        }

        public ActionResult CarregaModalConfirmacao(EnumStatusSolicitacao novoStatus, int idSol)
        {
            SolicitacaoDocumento sol = new SolicitacaoDocumento { IdSolicitacao = idSol, Status = novoStatus };
            return PartialView("_AlteracaoStatus", sol);
        }

        #region Métodos auxiliares

        private void PopularDropDowns()
        {
            //get todos os cursos
            var listCursos = cursoRepository.GetCursos().Select(item => new SelectListItem
            {
                Value = item.IdCurso.ToString(),
                Text = item.Nome.ToString(),
            });
            ViewBag.Cursos = new SelectList(listCursos, "Value", "Text");


            var listStatus = Enum.GetValues(typeof(EnumStatusSolicitacao)).
                Cast<EnumStatusSolicitacao>().Select(v => new SelectListItem
                {
                    Text = Util.Extension.EnumExtensions.GetEnumDescription(v),
                    Value = ((int)v).ToString()
                }).ToList();
            ViewBag.Status = new SelectList(listStatus, "Value", "Text");
        }

        private void PopularDropDownsCadastro()
        {
            var listCursos = cursoRepository.GetCursos().Select(item => new SelectListItem
            {
                Value = item.IdCurso.ToString(),
                Text = item.Nome.ToString(),
            });
            ViewBag.Cursos = new SelectList(listCursos, "Value", "Text");

            var listTiposDoc = tipoDocumentoRepository.listaTipos().Select(item => new SelectListItem
            {
                Value = item.IdTipoDoc.ToString(),
                Text = item.TipoDocumento1.ToString(),
            });
            ViewBag.TiposDocumento = new SelectList(listTiposDoc, "Value", "Text");
        }

        private void PopularDropDownAlunos(int idCurso)
        {
            //get todos alunos pelo id do curso
            var listAlunos = alunoRepository.GetAlunoByIdCurso(idCurso).Select(item => new SelectListItem
            {
                Value = item.IdAluno.ToString(),
                Text = item.Usuario.Nome.ToString(),
            });
            ViewBag.Alunos = new SelectList(listAlunos, "Value", "Text");
        }

        public JsonResult GetAlunosByIdCurso(int idCurso)
        {
            if (idCurso > 0)
            {
                var lstAlunos = alunoRepository.GetAlunoByIdCurso(idCurso);
                return Json(lstAlunos.Select(x => new { Value = x.IdAluno, Text = x.Usuario.Nome }));
            }
            return Json(null);
        }

        public object SalvarSolicitacao(SolicitacaoDocumento sol)
        {
            sol.Status = sol.IdSolicitacao > 0 ? sol.Status : EnumStatusSolicitacao.pendente;
            sol.DataAbertura = DateTime.Now;
            if (sol.IdSolicitacao == 0)
                sol.IdAlunoCurso = cursoRepository.GetAlunoCurso(sol.AlunoCurso.IdAluno, sol.AlunoCurso.IdCurso).IdAlunoCurso;
            sol.AlunoCurso = null;
            if(sol.IdSolicitacao == 0)
            {
                Documento d = new Documento();
                d.IdTipoDoc = (int)sol.TipoDocumento;
                d.IdAlunoCurso = sol.IdAlunoCurso;
                d.Data = sol.DataAbertura;
                d.NomeDocumento = "";

                sol.Documento = d;
            }
            

            if (ModelState.IsValid)
            {
                try
                {                    
                    string msg = solicitacaoRepository.PersisteSolicitacao(sol);

                    if (msg != "Erro")
                    {
                        Utilidades.SalvaLog(Utilidades.UsuarioLogado, EnumAcao.Persistir, sol, (sol.IdSolicitacao > 0? (int?)sol.IdSolicitacao:null));
                        return Json(new { Status = true, Type = "success", Message = "Solicitação salva com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
                }
            }
            else {
                return Json(new { Status = false, Type = "error", Message = "Campos inválidos" }, JsonRequestBehavior.AllowGet);
            }
        }

        public object ExcluirSolicitacao(SolicitacaoDocumento sol)
        {
            var s = solicitacaoRepository.GetSolicitacaoById(sol.IdSolicitacao);
            if (s.Status == EnumStatusSolicitacao.pendente) //regra q soh deleta se status for pendente
            {
                if (solicitacaoRepository.DeletaArquivo(s))
                {
                    Utilidades.SalvaLog(Utilidades.UsuarioLogado, EnumAcao.Excluir, s, s.IdSolicitacao);
                    return Json(new { Status = true, Type = "success", Message = "Solicitação deletada com sucesso!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = false, Type = "error", Message = "Só é possível realizar exclusão de solicitações pendentes." }, JsonRequestBehavior.AllowGet);
        }

        public object AlterarStatus(SolicitacaoDocumento solic)
        {
            try
            {
                var sol = solicitacaoRepository.GetSolicitacaoById(solic.IdSolicitacao);
                sol.Status = solic.Status;
                if(sol.Status == EnumStatusSolicitacao.pendente && !string.IsNullOrEmpty(sol.Documento.CaminhoDocumento))
                {
                    DirDoc.DeletaArquivo(sol.Documento.CaminhoDocumento);
                    sol.Documento.CaminhoDocumento = null;
                }               
               // string msg = solicitacaoRepository.AlteraStatus(sol, solic.Status);
                string msg = solicitacaoRepository.PersisteSolicitacao(sol);

                if (msg != "Erro")
                {
                    try
                    {
                        var acao = sol.Status == EnumStatusSolicitacao.cancelado ? "cancelada" :
                            sol.Status == EnumStatusSolicitacao.concluido ? "aprovada" :
                            sol.Status == EnumStatusSolicitacao.pendente ? "reprovada" : "";
                        var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/AlteracaoStatusSolicitacaoDocumento.cshtml");
                        string viewCode = System.IO.File.ReadAllText(url);

                        var html = RazorEngine.Razor.Parse(viewCode, sol);
                        var to = new[] { sol.AlunoCurso.Aluno.Usuario.E_mail };
                        var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                        Util.Email.EnviarEmail(from, to, "Solicitação de documento " + acao, html);
                    }
                    catch (Exception)
                    {
                    }

                    if(sol.Status == EnumStatusSolicitacao.concluido)
                    {
                        Utilidades.SalvaLog(Utilidades.UsuarioLogado, EnumAcao.Aprovar, sol, sol.IdSolicitacao);
                    }
                    else if (sol.Status == EnumStatusSolicitacao.pendente)
                    {
                        Utilidades.SalvaLog(Utilidades.UsuarioLogado, EnumAcao.Reprovar, sol, sol.IdSolicitacao);
                    }
                    else if (sol.Status == EnumStatusSolicitacao.cancelado)
                    {
                        Utilidades.SalvaLog(Utilidades.UsuarioLogado, EnumAcao.Cancelar, sol, sol.IdSolicitacao);
                    }

                    return Json(new { Status = true, Type = "success", Message = "Solicitação salva com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Baixa arquivo
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>retorna o arquivo pra download</returns>
        public FileResult Download(string nomeDoc)
        {
            Documento doc = documentoRepository.GetDocumentoByNome(nomeDoc);

            string nomeArquivo = doc.NomeDocumento;
            string extensao = Path.GetExtension(nomeArquivo);

            string contentType = "application/" + extensao.Substring(1);

            byte[] bytes = DirDoc.BaixaArquivo(doc);

            return File(bytes, contentType, nomeArquivo);
        }

        public static byte[] converterFileToArray(HttpPostedFileBase x)
        {
            MemoryStream tg = new MemoryStream();
            x.InputStream.CopyTo(tg);
            byte[] data = tg.ToArray();

            return data;
        }

        #endregion
    }
}