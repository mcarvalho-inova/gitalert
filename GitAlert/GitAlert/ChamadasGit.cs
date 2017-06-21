using Bonobo.Git.Server.Models;
using GitAlert.Classes;
using GitAlert.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;

namespace GitAlert
{
    public class ChamadasGit
    {
        /// <summary>
        /// Variavel para resgatar qual o caminhodo repositorio que está gerando erro.
        /// </summary>
        private static string RepoErro = string.Empty;

        private int contadorMudancas = 0;
        private int itemModificado = 0;

        public void CarregarCommits()
        {
            try
            {
                List<RepositorioModel> MeusRepo = RecuperarRepositorios();

                List<RepositorioModel> LogRepositorios = RetornarArquivoDeLog();

                foreach (RepositorioModel repositorio in MeusRepo)
                {
                    RepoErro = ConfigurationManager.AppSettings.Get("DefaultRepositoriesDirectory") + repositorio.RepoName;

                    using (var browser = new RepositoryBrowser(Path.Combine(ConfigurationManager.AppSettings.Get("DefaultRepositoriesDirectory"), repositorio.RepoName)))
                    {
                        IEnumerable<string> branchs = browser.GetBranches();

                        foreach (var NomeBranchs in branchs)
                        {
                            string referenceName = NomeBranchs;
                            int totalCount;

                            repositorio.Branchs.Add(new BranchModel
                            {
                                BranchName = NomeBranchs,
                                Commits = new List<CommitModel>(),
                                repositoryName = repositorio.RepoName
                            });

                            IEnumerable<RepositoryCommitModel> commits = browser.GetCommits(NomeBranchs, 1, 99, out referenceName, out totalCount);

                            foreach (var commit in commits)
                            {
                                itemModificado = 0;
                                contadorMudancas += 1;
                                var model = browser.GetCommitDetail(commit.ID);
                                List<RepositoryCommitChangeModel> mudancas = new List<RepositoryCommitChangeModel>();                           

                                if (model.Changes.Count() > 0 && model.Changes != null)
                                {
                                    mudancas = model.Changes.ToList();
                                }
                                
                              

                                Console.WriteLine("Commit: " + contadorMudancas);

                                repositorio.Branchs.First(x => x.BranchName == NomeBranchs).Commits.Add(new CommitModel
                                {
                                    Author = commit.Author,
                                    AuthorEmail = commit.AuthorEmail,
                                    Date = commit.Date,
                                    Name = commit.Name,
                                    Message = commit.Message,
                                    BranchName = NomeBranchs,
                                    Id = commit.ID,
                                    ItemsModificados = new List<ItemsCommitModel>(),
                                    RepoName = repositorio.RepoName
                                });

                                for (int i = 0; i < mudancas.Count(); i++)
                                {
                                    repositorio.Branchs.First(x => x.BranchName == NomeBranchs).Commits.First(y => y.Id == commit.ID).ItemsModificados.Add(new ItemsCommitModel
                                    {
                                        ChangeId = mudancas[i].ChangeId,
                                        Name = mudancas[i].Name,
                                        Path = mudancas[i].Path,
                                        Status = mudancas[i].Status.ToString()
                                    });

                                    Console.WriteLine("Item modificado: "+i);
                                    //Console.Clear();
                                    itemModificado += 1;
                                }

                               
                                Console.Clear();
                                break;
                            }
                        }
                    }
                }

                if (LogRepositorios != null)
                {
                    if (LogRepositorios.Count > 0)
                    {
                        VerificarMudancasNoRepositorio(MeusRepo, LogRepositorios);
                    }
                }

                SalvarLogUltimosCommits(MeusRepo);
            }
            catch (Exception ex)
            {
                using (StreamWriter file = new StreamWriter(ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString() + "ERRO_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true, System.Text.Encoding.Default))
                {
                    file.WriteLine("Exception: " + ex);
                    file.WriteLine("_-_-_");
                    file.WriteLine("Caminho do repositorio: " + RepoErro);
                    file.WriteLine("Mudança de numero: " + contadorMudancas.ToString());
                    file.WriteLine("Item modificado de numero: " + itemModificado.ToString());
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }

                throw;
            }
        }

        private void VerificarMudancasNoRepositorio(List<RepositorioModel> repositoriosAtuais, List<RepositorioModel> repositoriosAnteriores)
        {
            List<RepositorioModel> MudancasRepositorio = new List<RepositorioModel>();
            List<BranchModel> MudancasBranch = new List<BranchModel>();
            List<CommitModel> MudancasCommit = new List<CommitModel>();
            try
            {
                //for (int i = 0; i < repositoriosAtuais.Count; i++)
                //{
                //    //Verifica se os repositorios estão iguais
                //    if (repositoriosAtuais[i].RepoId == repositoriosAnteriores[i].RepoId)
                //    {
                //        repositoriosAnteriores[i].Branchs = repositoriosAnteriores[i].Branchs.OrderBy(x => x.BranchName).ToList();
                //        repositoriosAtuais[i].Branchs = repositoriosAtuais[i].Branchs.OrderBy(x => x.BranchName).ToList();

                //        for (int j = 0; j < repositoriosAtuais[i].Branchs.Count; j++)
                //        {
                //            if (repositoriosAnteriores[i].Branchs.Count > repositoriosAtuais[i].Branchs.Count)
                //            {
                //                foreach (var branch in repositoriosAnteriores[i].Branchs)
                //                {
                //                    if (!repositoriosAtuais[i].Branchs.Exists(x => x.BranchName == branch.BranchName))
                //                    {
                //                        MudancasBranch.Add(branch);
                //                        MudancasBranch.First(x => x.BranchName == branch.BranchName).BrachStatus = "Removida";
                //                        break;
                //                    }
                //                }
                //            }
                //            else if (repositoriosAtuais[i].Branchs.Count > repositoriosAnteriores[i].Branchs.Count)
                //            {
                //                MudancasBranch.Add(repositoriosAtuais[i].Branchs[j]);
                //                MudancasBranch.First(x => x.BranchName == repositoriosAtuais[i].Branchs[j].BranchName).BrachStatus = "Adicionada";
                //                repositoriosAnteriores[i].Branchs.Add(repositoriosAtuais[i].Branchs[j]);
                //                repositoriosAnteriores[i].Branchs = repositoriosAnteriores[i].Branchs.OrderBy(x => x.BranchName).ToList();
                //                repositoriosAtuais[i].Branchs = repositoriosAtuais[i].Branchs.OrderBy(x => x.BranchName).ToList();
                //            }
                //            //Verifica se as branchs estão iguais
                //            if (repositoriosAtuais[i].Branchs[j].BranchName == repositoriosAnteriores[i].Branchs[j].BranchName)
                //            {
                //                for (int l = 0; l < repositoriosAtuais[i].Branchs[j].Commits.Count; l++)
                //                {
                //                    if (repositoriosAtuais[i].Branchs[j].Commits[l].Date != repositoriosAnteriores[i].Branchs[j].Commits[l].Date)
                //                    {
                //                        MudancasCommit.Add(repositoriosAtuais[i].Branchs[j].Commits[l]);
                //                        break;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        MudancasRepositorio.Add(repositoriosAtuais[i]);
                //        break;
                //    }
                //}

                //if (MudancasRepositorio.Count > 0 || MudancasBranch.Count > 0 || MudancasCommit.Count > 0)
                //{
                //    EnviarEmail(MudancasRepositorio, MudancasCommit, MudancasBranch);
                //}

                foreach (var repositorio in repositoriosAtuais)
                {
                    if (repositoriosAnteriores.Exists(x => x.RepoName == repositorio.RepoName))
                    {
                        foreach (var branch in repositorio.Branchs)
                        {
                            if (repositoriosAnteriores.First(x => x.RepoName == repositorio.RepoName).Branchs.Exists(x => x.BranchName == branch.BranchName))
                            {
                                foreach (var commit in branch.Commits)
                                {
                                    if (!repositoriosAnteriores.First(x => x.RepoName == repositorio.RepoName).Branchs.First(y => y.BranchName == branch.BranchName).Commits.Exists(i => i.Id == commit.Id))
                                    {
                                        MudancasCommit.Add(commit);
                                    }
                                }
                            }
                        }
                    }

                    #region Controle de Branchs Adicionadas e Removidas

                    if (repositoriosAnteriores.Exists(x => x.RepoName == repositorio.RepoName))
                    {
                        if (repositorio.Branchs.Count < repositoriosAnteriores.First(x => x.RepoName == repositorio.RepoName).Branchs.Count)
                        {
                            foreach (var branchAnterior in repositoriosAnteriores.First(x => x.RepoName == repositorio.RepoName).Branchs)
                            {
                                if (!repositorio.Branchs.Exists(x => x.BranchName == branchAnterior.BranchName))
                                {
                                    MudancasBranch.Add(branchAnterior);
                                    MudancasBranch.First(x => x.BranchName == branchAnterior.BranchName).BrachStatus = "Removido";
                                }
                            }
                        }
                        else if (repositorio.Branchs.Count > repositoriosAnteriores.First(x => x.RepoName == repositorio.RepoName).Branchs.Count)
                        {
                            foreach (var branch in repositorio.Branchs)
                            {
                                if (!repositoriosAnteriores.First(x => x.RepoName == repositorio.RepoName).Branchs.Exists(x => x.BranchName == branch.BranchName))
                                {
                                    MudancasBranch.Add(branch);
                                    MudancasBranch.First(x => x.BranchName == branch.BranchName).BrachStatus = "Adicionada";
                                }
                            }
                        }
                        else if (repositorio.Branchs.Count == repositoriosAnteriores.First(x => x.RepoName == repositorio.RepoName).Branchs.Count)
                        {
                            foreach (var branch in repositorio.Branchs)
                            {
                                if (!repositoriosAnteriores.First(x => x.RepoName == repositorio.RepoName).Branchs.Exists(x => x.BranchName == branch.BranchName))
                                {
                                    MudancasBranch.Add(branch);
                                    MudancasBranch.First(x => x.BranchName == branch.BranchName).BrachStatus = "Adicionada";
                                }
                            }
                        }
                    }
                }

                #endregion Controle de Branchs Adicionadas e Removidas

                #region Controle de Repositorios Adicionados e removidos

                if (repositoriosAtuais.Count < repositoriosAnteriores.Count)
                {
                    foreach (var repoAntigo in repositoriosAnteriores)
                    {
                        if (!repositoriosAtuais.Exists(x => x.RepoName == repoAntigo.RepoName))
                        {
                            MudancasRepositorio.Add(repoAntigo);
                            MudancasRepositorio.First(x => x.RepoName == repoAntigo.RepoName).RepoStatus = "Removido";
                        }
                    }
                }
                else if (repositoriosAtuais.Count > repositoriosAnteriores.Count)
                {
                    foreach (var repo in repositoriosAtuais)
                    {
                        if (!repositoriosAnteriores.Exists(x => x.RepoName == repo.RepoName))
                        {
                            MudancasRepositorio.Add(repo);
                            MudancasRepositorio.First(x => x.RepoName == repo.RepoName).RepoStatus = "Adicionado";
                        }
                    }
                }
                else if (repositoriosAtuais.Count == repositoriosAnteriores.Count)
                {
                    foreach (var repo in repositoriosAtuais)
                    {
                        if (!repositoriosAnteriores.Exists(x => x.RepoName == repo.RepoName))
                        {
                            MudancasRepositorio.Add(repo);
                            MudancasRepositorio.First(x => x.RepoName == repo.RepoName).RepoStatus = "Adicionada";
                        }
                    }
                }

                #endregion Controle de Repositorios Adicionados e removidos

                if (MudancasRepositorio.Count > 0 || MudancasBranch.Count > 0 || MudancasCommit.Count > 0)
                {
                    EnviarEmail(MudancasRepositorio, MudancasCommit, MudancasBranch);
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter file = new StreamWriter(ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString() + "ERRO_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true, System.Text.Encoding.Default))
                {
                    file.WriteLine("Exception: " + ex);
                    file.WriteLine("_-_-_");
                    file.WriteLine("Caminho do repositorio: " + RepoErro);
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }

                throw;
            }
        }

        /// <summary>
        /// Deserializa o arquivo de log feito no dia.
        /// </summary>
        /// <typeparam name="T">Aqui vai o tipo do parametro que vai serializar</typeparam>
        /// <param name="NomeDoArquivo"></param>
        /// <returns></returns>
        public T DeSerializarAlteracoes<T>(string NomeDoArquivo)
        {
            if (string.IsNullOrEmpty(NomeDoArquivo)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(NomeDoArquivo);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter file = new StreamWriter(ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString() + "ERRO_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true, System.Text.Encoding.Default))
                {
                    file.WriteLine("Exception: " + ex);
                    file.WriteLine("_-_-_");
                    file.WriteLine("Caminho do repositorio: " + RepoErro);
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }
            }

            return objectOut;
        }

        public List<RepositorioModel> RecuperarRepositorios()
        {
            try
            {
                DataTable dt = new DataTable();
                List<RepositorioModel> Repositorios = new List<RepositorioModel>();
                String insSQL = "select * from Repository";
                //string connectionStringName = ConfigurationManager.AppSettings.Get("BonoboGitServerContext");
                string strConn = ConfigurationManager.ConnectionStrings["BonoboGitServerContext"].ConnectionString;

                SQLiteConnection conn = new SQLiteConnection(strConn);

                SQLiteDataAdapter da = new SQLiteDataAdapter(insSQL, strConn);
                da.Fill(dt);

                foreach (DataRow item in dt.Rows)
                {
                    Repositorios.Add(new RepositorioModel
                    {
                        RepoId = item.ItemArray[0].ToString(),
                        RepoName = item.ItemArray[1].ToString(),
                        RepoDescription = item.ItemArray[2].ToString(),
                        Branchs = new List<BranchModel>()
                    });
                }
                return Repositorios;
            }
            catch (Exception ex)
            {
                using (StreamWriter file = new StreamWriter(ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString() + "ERRO_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true, System.Text.Encoding.Default))
                {
                    file.WriteLine("Exception: " + ex);
                    file.WriteLine("_-_-_");
                    file.WriteLine("Caminho do repositorio: " + RepoErro);
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }
                throw;
            }
        }

        public List<RepositorioModel> RetornarArquivoDeLog()
        {
            bool retorno = false;
            string caminhoDoLog = ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString();
            string arquivo = caminhoDoLog + "Alterações Repositorio " + DateTime.Today.ToString("dd-MM-yyyy") + ".log";
            List<RepositorioModel> RepositoriosSalvos = new List<RepositorioModel>();

            retorno = File.Exists(arquivo) ? true : false;

            if (retorno)
            {
                RepositoriosSalvos = DeSerializarAlteracoes<List<RepositorioModel>>(arquivo);
            }

            return RepositoriosSalvos;
        }

        public void SalvarLogUltimosCommits(List<RepositorioModel> Repositorios)
        {
            try
            {
                string caminhoDoLog = ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString();
                string file = caminhoDoLog + "Alterações Repositorio " + DateTime.Today.ToString("dd-MM-yyyy") + ".log";
                SerializarAlteracoes(Repositorios, file);
            }
            catch (Exception ex)
            {
                using (StreamWriter file = new StreamWriter(ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString() + "ERRO_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true, System.Text.Encoding.Default))
                {
                    file.WriteLine("Exception: " + ex);
                    file.WriteLine("_-_-_");
                    file.WriteLine("Caminho do repositorio: " + RepoErro);
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }
                throw;
            }
        }

        /// <summary>
        /// Serializa o arquivo de repositorios para o log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjetoSerializado"></param>
        /// <param name="NomeDoArquivo"></param>
        public void SerializarAlteracoes<T>(T ObjetoSerializado, string NomeDoArquivo)
        {
            if (ObjetoSerializado == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(typeof(List<RepositorioModel>));
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, ObjetoSerializado);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(NomeDoArquivo);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter file = new StreamWriter(ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString() + "ERRO_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true, System.Text.Encoding.Default))
                {
                    file.WriteLine("Exception: " + ex);
                    file.WriteLine("_-_-_");
                    file.WriteLine("Caminho do repositorio: " + RepoErro);
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }
                throw;//Log exception here
            }
        }

        public static void EnviarEmail(List<RepositorioModel> mudancasRepositorio, List<CommitModel> mudancasCommit, List<BranchModel> mudancasBranch)
        {
            try
            {
                string Body = string.Empty;
                MailMessage message = new MailMessage();
                message.From = new MailAddress(ConfigurationManager.AppSettings.Get("MAILFROM"));
                message.To.Add(ConfigurationManager.AppSettings.Get("MAILTO"));
                message.Subject = ConfigurationManager.AppSettings.Get("SUBJECT").ToString();
                message.IsBodyHtml = true;

                Body = "<h3 style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif;'>Prezado colaborador,</h3>" +
                       "<p style='margin:0px;font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif;font-size:14px;'>Este é um e-mail automatico do sistema de pushs do repositorio que " + ConfigurationManager.AppSettings.Get("PARTNER").ToString() + " gerencia. </p>" +
                       "<p style='margin:0px;font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif;font-size:14px;'>Veja abaixo os itens pertinentes ao seu projeto e esteja atento à qualquer atualização que possa impactar nas suas atividades!</p>" +
                       "</br>" +
                       "<div style=' text-align:center; background-color:#F2F2F2;font-weight:bold;font-size:18px;font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif;Width:800px;'><p>Ultima verificação: " + DateTime.Now.ToString() + "</p></div>";

                if (mudancasRepositorio.Count > 0)
                {
                    foreach (var item in mudancasRepositorio)
                    {
                        Body += "<table style='border-collapse:collapse;border-spacing:0;border-color:#000000;border: black solid 2px; margin-bottom: 30px;width:800px'><tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;font-weight:normal;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#4472c4;vertical-align:top'  colspan='4'>Repositórios</td></tr>" +
                            "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top' colspan='3'>Nome</td><td style='font-family:Tahoma, Geneva, sans-serif !important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#ccc;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top'>Status</td></tr>" +
                            "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top' colspan='3'>" + item.RepoName + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top'>" + item.RepoStatus + "</td></tr>" +
                            "</table>";
                    }
                }

                if (mudancasBranch.Count > 0)
                {
                    foreach (var item in mudancasBranch)
                    {
                        Body += "<table style='border-collapse:collapse;border-spacing:0;border-color:#000000;border: black solid 2px; margin-bottom: 30px;width:800px'><tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;font-weight:normal;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#4472c4;vertical-align:top'  colspan='4'>" + item.repositoryName + "</td></tr>" +
                            "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top' colspan='3'>Branch</td><td style='font-family:Tahoma, Geneva, sans-serif !important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#ccc;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top'>Status</td></tr>" +
                            "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top' colspan='3'>" + item.BranchName + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top' >" + item.BrachStatus + "</td></tr>" +
                            "</table>";
                    }
                }

                if (mudancasCommit.Count > 0)
                {
                    foreach (var item in mudancasCommit)
                    {
                        Body += "<table style='border-collapse:collapse;border-spacing:0;border-color:#000000;border: black solid 2px; margin-bottom: 30px;width:800px'><tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;font-weight:normal;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#4472c4;vertical-align:top' colspan='4'>" + item.RepoName + "</td></tr>" +
                            "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top'>Branch</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top'>Autor</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top'>Mensagem</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top'>Data</td></tr>" +
                            "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top'>" + item.BranchName + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top'>" + item.Author + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top'>" + item.Message + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;text-align:center;vertical-align:top'>" + item.Date.ToString() + "</td></tr>" +
                            "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:15px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#ffffff;background-color:#44546a;text-align:center;vertical-align:top' colspan='4'>Arquivos Modificados</td></tr>";

                        foreach (var arquivo in item.ItemsModificados)
                        {
                            if (arquivo.Status == "Deleted")
                            {
                                Body += "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top' colspan='3'>" + arquivo.Path + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;text-align:center;vertical-align:top;color:red;'>" + arquivo.Status + "</td></tr>";
                            }
                            else if (arquivo.Status == "Added")
                            {
                                Body += "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top' colspan='3'>" + arquivo.Path + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;text-align:center;vertical-align:top;color:#002060;'>" + arquivo.Status + "</td></tr>";
                            }
                            else if (arquivo.Status == "Modified")
                            {
                                Body += "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top' colspan='3'>" + arquivo.Path + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;text-align:center;vertical-align:top;color:#7030A0;'>" + arquivo.Status + "</td></tr>";
                            }
                            else
                            {
                                Body += "<tr><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;vertical-align:top' colspan='3'>" + arquivo.Path + "</td><td style='font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif!important;;font-size:12px;padding:5px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;border-color:#000000;color:#333;background-color:#fff;text-align:center;vertical-align:top;'>" + arquivo.Status + "</td></tr>";
                            }
                        }

                        Body += "</table></br></br>";
                    }

                    Body += "<h4 style='margin:0px;font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif;'>InovaBox Git Alert 1.0</h4>" +
                        "<p  style='margin:0px;font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif;'>Copyright ₢ 2017 - Inova e-Business Apoio Administrativo e Serviços de Informática Ltda</p>" +
                        "<p> </p><p  style='margin:0px;font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif;'>Caso você tenha recebido este e-mail por engano ou não faça parte da equipe de desenvolvimento por favor notifique imediatamente</p>" +
                        "<p style='margin:0px;font-family:Calibri,Candara,Segoe,Segoe UI,Optima,Arial,sans-serif;'>a gerência pelo e-mail diretoria@inovaebiz.com.br e apague este e-mail.</p>";
                }

                message.Body = Body;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.UseDefaultCredentials = true;

                smtpClient.Host = ConfigurationManager.AppSettings.Get("SMTP").ToString();
                smtpClient.Port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("PORT"));
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings.Get("USERNAMEMAIL").ToString(), ConfigurationManager.AppSettings.Get("PASSWORDMAIL").ToString());
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                using (StreamWriter file = new StreamWriter(ConfigurationManager.AppSettings.Get("DefaultLogDirectory").ToString() + "ERRO_" + DateTime.Today.ToString("dd-MM-yyyy") + ".log", true, System.Text.Encoding.Default))
                {
                    file.WriteLine("Exception: " + ex);
                    file.WriteLine("_-_-_");
                    file.WriteLine("Caminho do repositorio: " + RepoErro);
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }
                throw;
            }
        }
    }
}