﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControleDocumentosLibrary;
using ControleDocumentos.Util.Extension;

namespace ControleDocumentos.Repository
{
    public class LogsRepository
    {
        DocumentosModel db = new DocumentosModel();
        public bool SalvarLog(Logs log)
        {
            db.Logs.Add(log);

            try
            {
                return db.SaveChanges() > 0;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<Logs> GetLogByUserId(string id)
        {
            return db.Logs.Where(x => x.IdUsuario == id).ToList();
        }

        public List<Logs> GetLogByObj<T>(int idObj, T objeto)
        {
            string nomeObj = objeto.GetType().Name;
            List<Logs> logs = db.Logs.Where(x => x.IdObjeto == idObj && x.TipoObjeto == EnumExtensions.GetValueFromDescription<EnumTipoObjeto>(nomeObj)).ToList();

            return logs;
        }

        public List<Logs> GetLogByData(DateTime data)
        {
            return db.Logs.Where(x => x.Data.ToShortDateString() == data.ToShortDateString()).ToList();
        }
    }
}