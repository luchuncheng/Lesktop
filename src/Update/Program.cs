using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Diagnostics;

namespace Update
{
    using Properties;

    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                XmlDocument setting_conf_doc = new XmlDocument();
                setting_conf_doc.Load(AppDomain.CurrentDomain.BaseDirectory + "Setting.conf");
                XmlNodeList service_url_node = setting_conf_doc.DocumentElement.GetElementsByTagName("ServiceUrl");
                string service_url = null;
                if (service_url_node.Count > 0 && service_url_node[0].NodeType == XmlNodeType.Element)
                {
                    service_url = (service_url_node[0] as XmlElement).InnerText;
                }
                else
                {
                    return;
                }

                string client_filename = AppDomain.CurrentDomain.BaseDirectory + "Client.exe";

                string update_config_localpath = AppDomain.CurrentDomain.BaseDirectory + "latest.xml";
                string update_config_url = service_url + "/Update/latest.xml?" + Math.Abs(DateTime.Now.Ticks).ToString();

                WebClient client = new WebClient();
                client.DownloadFile(update_config_url, update_config_localpath);
                XmlDocument updata_config = new XmlDocument();
                updata_config.Load(update_config_localpath);
                AssemblyName client_assembly = AssemblyName.GetAssemblyName(client_filename);
                Version current_version = client_assembly.Version;
                XmlElement update_node = updata_config.GetElementsByTagName("Update")[0] as XmlElement;
                Version latestVersion = new Version(update_node.GetAttribute("Latest"));
                if (current_version < latestVersion)
                {
                    UpdateProgress updata_progress = new UpdateProgress(
                        update_node.GetAttribute("URL"),
                        AppDomain.CurrentDomain.BaseDirectory + "update.zip"
                    );

                    if (updata_progress.ShowDialog() == DialogResult.OK)
                    {
                        bool auto_update = true;
                        while (!DeleteFile(client_filename))
                        {
                            if (MessageBox.Show(Resources.strTip3, Resources.strTip, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                            {
                                auto_update = false;
                                break;
                            }
                        }
                        if (auto_update)
                        {
                            try
                            {
                                CompressUtility.UnZip(
                                    AppDomain.CurrentDomain.BaseDirectory + "update.zip",
                                    AppDomain.CurrentDomain.BaseDirectory
                                );
                                Process.Start(client_filename);
                            }
                            catch
                            {
                                MessageBox.Show("更新应用程序失败，请您手动解压安装目录下的update.zip文件更新应用程序！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Resources.strTip1, Resources.strTip);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.strError);
            }
        }

        public static bool DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
    static class CompressUtility
    {
        public static void UnZip(string fileName, string target)
        {
            if (!target.EndsWith("\\")) target += "\\";
            using (ZipInputStream inputStream = new ZipInputStream(File.OpenRead(fileName)))
            {
                try
                {
                    ZipEntry theEntry;
                    while ((theEntry = inputStream.GetNextEntry()) != null)
                    {
                        if (theEntry.IsDirectory)
                        {
                            Directory.CreateDirectory(target + theEntry.Name);
                        }
                        else if (theEntry.IsFile)
                        {
                            using (FileStream fileWrite = new FileStream(target + theEntry.Name, FileMode.Create, FileAccess.Write))
                            {
                                try
                                {
                                    byte[] buffer = new byte[2048];
                                    int size;
                                    while (true)
                                    {
                                        size = inputStream.Read(buffer, 0, buffer.Length);
                                        if (size > 0)
                                            fileWrite.Write(buffer, 0, size);
                                        else
                                            break;
                                    }
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                                finally
                                {
                                    fileWrite.Close();
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    inputStream.Close();
                }
            }
        }
   
    }
}