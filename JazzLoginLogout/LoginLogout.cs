using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JazzLoginLogout
{
    /// <summary>Handles login and logout for creation and edit of jazz application data
    /// <para>Only one person at the time is allowed to change data for some jazz applications.</para>
    /// <para>There is a login/logout log file on the jazz server.</para>
    /// <para>For every login and logout a line will be added to the file.</para>
    /// <para>These lines have a time stamp and the name of the computer.</para>
    /// <para>The last line of the file is used to determine if it is possible to login.</para>
    /// </summary>
    static public class LoginLogout
    {
        #region Member variables
        /// <summary>Input data</summary>
        static private LoginLogoutInput m_login_logout_input = null;

        /// <summary>Flag telling if jazz data has been checked out.</summary>
        static private bool m_data_checked_out = false;

        /// <summary>Get or set the flag telling if data has been checked out</summary>
        static public bool DataCheckedOut
        { get { return m_data_checked_out; } set { m_data_checked_out = value; } }

        /// <summary>Encoding used for read and write of files</summary>
        static private Encoding m_file_encoding = Encoding.UTF8;

        #endregion // Member variables

        /// <summary>Initialization
        /// <para></para>
        /// </summary>
        /// <param name="i_login_logout_input">Input data for chckin and checkout</param>
        /// <param name="o_error">Error message</param>
        static public bool Init(LoginLogoutInput i_login_logout_input, out string o_error)
        {
            bool ret_init = true;
            o_error = "";

            if (!_CheckInput(i_login_logout_input, out o_error))
            {
                return false;
            }

            m_login_logout_input = i_login_logout_input;

            DataCheckedOut = false;

            return ret_init;

        } // Init


        /// <summary>Register in the login-logout file that data will be edited and/or created
        /// <para>1. The checkin/checkout file is downloaded (with function _DownloadCheckOutInLogFile).</para>
        /// <para>2. The fields of the last row in the file is retrieved (with function _GetLastRowCheckInOutFields)</para>
        /// <para>3. Return with error if data already is checked out by somebody else.</para>
        /// <para>4. Append logout line with function _AppendLogFileRow</para>
        /// <para>5. Upload the checkin/checkout file with function UploadFile in class Ftp.UpLoad.</para>
        /// <para>6. The flag m_data_checked_out is set to true.</para>
        /// </summary>
        /// <param name="i_force_checkout">Force checkout when data already is checked out</param>
        /// <param name="o_already_checked_out">Flag telling that data already is checked out</param>
        /// <param name="o_message">Message for successful execution</param>
        /// <param name="o_error">Error message</param>
        static public bool Checkout(bool i_force_checkout, out bool o_already_checked_out, out string o_message, out string o_error)
        {
            o_already_checked_out = false;
            o_message = @"";
            o_error = @"";

            if (!_CheckInput(m_login_logout_input, out o_error)) { return false; }

            string server_file_name = @"";
            string local_file_name = @"";
            if (!_DownloadCheckOutInLogFile(out server_file_name, out local_file_name, out o_error)) { return false; }

            string row_start = "";
            string row_time = "";
            string row_machine = "";
            if (!_GetLastRowCheckInOutFields(local_file_name, out row_start, out row_time, out row_machine, out o_error))
            {
                return false;
            }

            string comp_string_in = m_login_logout_input.CheckInStart;
            string comp_string_out = m_login_logout_input.CheckOutStart;

            if (String.Compare(comp_string_in, row_start, false) == 0)
            {
                if (!_AppendLogFileRow(local_file_name, comp_string_out, out o_error))
                    return false;
            }
            else if (i_force_checkout && String.Compare(comp_string_out, row_start, false) == 0)
            {
                o_already_checked_out = true;
                o_message = m_login_logout_input.MsgDataAreForcedCheckedOut;
            }
            else if (String.Compare(comp_string_out, row_start, false) == 0)
            {
                o_already_checked_out = true;
                o_error = m_login_logout_input.ErrMsgCheckedOutBy + row_machine + "\n" +
                          row_time + "\n\n" + m_login_logout_input.MsgForceCheckOut;
                return false;
            }
            else
            {
                o_error = @"Programming error: Last row start element in login-logout is " + row_start;
                return false;
            }



            Ftp.UpLoad ftp_upload = new Ftp.UpLoad(m_login_logout_input.FtpHost, m_login_logout_input.FtpUser, m_login_logout_input.FtpPassword);

            if (!ftp_upload.UploadFile(server_file_name, local_file_name, out o_error))
            {
                o_error = m_login_logout_input.ErrMsgUploadLogfile;
                return false;
            }

            DataCheckedOut = true;

            if (!o_already_checked_out)
            {
                o_message = m_login_logout_input.MsgDataAreCheckedOut;
            }
            

            return true;

        } // Checkout


        /// <summary>Check in
        /// <para>Function for the case that data is checked out but the user don't want to upload (save) the file.</para>
        /// <para>The checkin/checkout log file is downloaded with FTP and a checkin line is appended.</para>
        /// <para>The checkin/checkout log file is upoaded with UploadFile in class Ftp.UpLoad.</para>
        /// <para>The flag m_data_checked_out is set to false.</para>
        /// </summary>
        /// <param name="i_force_checkout">Force checkout when data already is checked out</param>
        /// <param name="o_message">Message for successful execution</param>
        /// <param name="o_error">Error description</param>
        static public bool Checkin(bool i_force_checkin, out string o_message, out string o_error)
        {
            o_error = @"";
            o_message = @"";

            if (false == DataCheckedOut)
            {
                o_error = m_login_logout_input.ErrMsgDataNotCheckedOut;
                return false;
            }

            string local_file_name = "";
            string server_file_name = "";

            if (!_DownloadCheckOutInLogFile(out server_file_name, out local_file_name, out o_error))
            {
                return false;
            }

            if (!i_force_checkin)
            {
                if (!_DataIsCheckedOut(local_file_name, out o_error))
                {
                    return false;
                }
            }

            Ftp.UpLoad ftp_upload = new Ftp.UpLoad(m_login_logout_input.FtpHost, m_login_logout_input.FtpUser, m_login_logout_input.FtpPassword);

            if (!_AppendLogFileRow(local_file_name, m_login_logout_input.CheckInStart, out o_error))
                return false;

            if (!ftp_upload.UploadFile(server_file_name, local_file_name, out o_error))
            {
                o_error = m_login_logout_input.ErrMsgUploadLogfile;
                return false;
            }

            DataCheckedOut = false;

            o_message = m_login_logout_input.MsgDataAreCheckedIn;

            return true;
        } // Checkin

        #region Private functions

        /// <summary>Returns false if data not is checked out by this computer
        /// <para>1. The fields of the last row in the file is retrieved (with function _GetLastRowCheckInOutFields)</para>
        /// <para>2. Return with error if computer field name not is equal to this computer.</para>
        /// </summary>
        /// <param name="i_local_file_name">Full local name of the checkin-checkout file</param>
        /// <param name="o_error">Error description</param>
        static private bool _DataIsCheckedOut(string i_local_file_name, out string o_error)
        {
            o_error = "";

            string row_start = "";
            string row_time = "";
            string row_machine = "";
            if (!_GetLastRowCheckInOutFields(i_local_file_name, out row_start, out row_time, out row_machine, out o_error))
            {
                return false;
            }

            string comp_string_in = m_login_logout_input.CheckInStart;
            string comp_string_out = m_login_logout_input.CheckOutStart;

            if (String.Compare(comp_string_in, row_start, false) == 0)
            {
                o_error = @"_DataIsCheckedOut Programming error: Data is not checked out";
                return false;
            }
            else if (String.Compare(comp_string_out, row_start, false) == 0)
            {
                string machine = System.Environment.MachineName;

                if (String.Compare(machine, row_machine, false) != 0)
                {
                    o_error = @"_DataIsCheckedOut Programming error: Data is checked out by " +
                             row_machine + @" and not you (" + machine + @")";
                    return false;
                }
            }
            else
            {
                o_error = @"_DataIsCheckedOut Programming error: Last row start element in login-logout is " + row_start;
                return false;
            }

            return true;
        } // _DataIsCheckedOut


        /// <summary>Append row to login-logout file
        /// <para>Time and machine (computer) is added to the input string. </para>
        /// <para>The line is appended to the file.</para>
        /// </summary>
        /// <param name="i_local_file_name">Full input file name</param>
        /// <param name="i_start_append_row">Start string for the append row (login or logout)</param>
        /// <param name="o_error">Error description</param>
        static private bool _AppendLogFileRow(string i_local_file_name, string i_start_append_row, out string o_error)
        {
            o_error = "";

            string append_row = "\n" + i_start_append_row + @" " + _YearMonthDayHourMinSec() +
                                @" " + System.Environment.MachineName;

            try
            {
                using (StreamWriter writer = File.AppendText(i_local_file_name))
                {
                    writer.Write(append_row);
                }
            }

            catch (Exception e)
            {
                o_error = " Unhandled Exception " + e.GetType() + " occurred at " + DateTime.Now + "!";
                return false;
            }

            return true;
        } // _AppendLogFileRow

        /// <summary>Returns date and time (year_month_day_hour_Minute_Second) as a string</summary>
        static private string _YearMonthDayHourMinSec()
        {
            DateTime current_time = DateTime.Now;
            int now_year = current_time.Year;
            int now_month = current_time.Month;
            int now_day = current_time.Day;
            int now_hour = current_time.Hour;
            int now_minute = current_time.Minute;
            int now_second = current_time.Second;

            string time_text = now_year.ToString() + "_" + _IntToString(now_month) + "_" + _IntToString(now_day) + "_" + _IntToString(now_hour) + "_" + _IntToString(now_minute) + "_" + _IntToString(now_second);

            return time_text;
        } // _YearMonthDayHourMinSec

        /// <summary>Returns date and time as a string with a '0' added if input number is less that ten (10)</summary>
        static private string _IntToString(int i_int)
        {
            string time_text = i_int.ToString();

            if (i_int <= 9)
            {
                time_text = "0" + time_text;
            }

            return time_text;
        } // _IntToString

        /// <summary>Get the fields of the last row of the checkin-checkout file 
        /// <para>1. Get last row (call of function _GetLastRow).</para>
        /// <para>2. Retrieve the field values. The start of each row is defined by</para>
        /// <para>   strings CheckInStart and CheckOutStart</para>
        /// </summary>
        /// <param name="i_local_file_name">Full input file name</param>
        /// <param name="o_start">Start field of the row</param>
        /// <param name="o_time">Time field of the row</param>
        /// <param name="o_machine">Machine name field of the row</param>
        /// <param name="o_error">Error description</param>
        static private bool _GetLastRowCheckInOutFields(string i_local_file_name, out string o_start, out string o_time, out string o_machine, out string o_error)
        {
            o_start = "";
            o_time = "";
            o_machine = "";
            o_error = "";

            string last_row = "";

            if (!_GetLastRow(i_local_file_name, out last_row, out o_error))
            {
                return false;
            }

            string comp_string_in = m_login_logout_input.CheckInStart;
            string start_string_in = last_row.Substring(0, comp_string_in.Length);

            string comp_string_out = m_login_logout_input.CheckOutStart;
            string start_string_out = last_row.Substring(0, comp_string_out.Length);

            if (String.Compare(comp_string_in, start_string_in, false) == 0)
            {
                o_start = comp_string_in;
            }
            else if (String.Compare(comp_string_out, start_string_out, false) == 0)
            {
                o_start = comp_string_out;
            }
            else
            {
                o_error = @"Programming error: Last row in login-logout is " + last_row;
                return false;
            }

            string current_field = "";

            for (int i_char = o_start.Length + 1; i_char < last_row.Length; i_char++)
            {
                string current_char = last_row.Substring(i_char, 1);

                if (current_char.CompareTo(" ") == 0)
                {
                    if (o_time.CompareTo("") == 0)
                    {
                        o_time = current_field;
                        current_field = "";
                    }
                }
                else
                {
                    current_field = current_field + current_char;
                }
            }

            o_machine = current_field;

            return true;
        } // _GetLastRowCheckInOutFields

        /// <summary>Get last row of the input file </summary>
        /// <para>File is opened and all rows are read. The last (not empy) row is returned.</para>
        /// <param name="i_local_file_name">Full input file name</param>
        /// <param name="o_last_row">Last (non-empty) row of the file</param>
        /// <param name="o_error">Error description</param>
        static private bool _GetLastRow(string i_local_file_name, out string o_last_row, out string o_error)
        {
            o_last_row = "";
            o_error = "";

            if (!File.Exists(i_local_file_name))
            {
                o_error = @"File: " + i_local_file_name + @" does not exist. Programming error";
                return false;
            }

            try
            {
                using (FileStream file_stream = new FileStream(i_local_file_name, FileMode.Open, FileAccess.Read, FileShare.Read))
                // Without System.Text.Encoding.UTF8 there are problems with ä ö ü. With Encoding.Default it worked in some computers
                // Alternatives Encoding.Default, Encoding.UTF8, Encoding.Unicode, Encoding.UTF32, Encoding.UTF7
                // using (StreamReader stream_reader = new StreamReader(file_stream, System.Text.Encoding.Default))
                using (StreamReader stream_reader = new StreamReader(file_stream))
                {
                    while (stream_reader.Peek() >= 0)
                    {
                        string current_row = stream_reader.ReadLine();

                        if (current_row.Trim() == "")
                        {
                            // A line with only spaces.
                            break;
                        }
                        else
                        {
                            o_last_row = current_row;
                        }

                    } // while
                }
            }


            catch (FileNotFoundException) { o_error = "File not found"; return false; }
            catch (DirectoryNotFoundException) { o_error = "Directory not found"; return false; }
            catch (InvalidOperationException) { o_error = "Invalid operation"; return false; }
            catch (InvalidCastException) { o_error = "invalid cast"; return false; }
            catch (Exception e)
            {
                o_error = " Unhandled Exception " + e.GetType() + " occurred at " + DateTime.Now + "!";
                return false;
            }

            return true;
        } // _GetLastRow

        /// <summary>Download the checkin/checkout file from the server with FTP
        /// <para>It is recommended that the calling function checks that there is an Internet connection</para>
        /// <para>1. The checkin/checkout file is downloaded with function DownloadFile in class Ftp.DownLoad.</para>
        /// </summary>
        /// <param name="o_server_file_name">Server checkin-checkout file name</param>
        /// <param name="o_local_file_name">Full name of checkin-checkout file</param>
        /// <param name="o_error">Error description</param>
        static private bool _DownloadCheckOutInLogFile(out string o_server_file_name, out string o_local_file_name, out string o_error)
        {
            o_local_file_name = _GetCheckInOutLogFileName();
            o_server_file_name = _GetServerCheckInOutLogFileName(Path.GetFileName(o_local_file_name));

            o_error = "";

            Ftp.DownLoad ftp_download = new Ftp.DownLoad(m_login_logout_input.FtpHost, m_login_logout_input.FtpUser, m_login_logout_input.FtpPassword);

            /*QQQQQ
            string str_uri = @"http://" + m_login_logout_input.FtpHost;

            if (!Ftp.InternetUtil.IsConnectionAvailable(str_uri, out o_error))
            {
                o_error = m_login_logout_input.ErrMsgNoInternetConnection;
                return false;
            }
            QQQ*/

            if (!ftp_download.DownloadFile(o_server_file_name, o_local_file_name, out o_error))
            {
                o_error = m_login_logout_input.ErrMsgNoCheckInOutLogFileDownload;
                return false;
            }

            return true;

        } // _DownloadCheckOutInLogFile


        /// <summary>Returns the server file name for the checkin-checkout logfile
        /// <para>The ftp directory is the FtpUser name is for instance appdata@jazzliveaarau.ch</para>
        /// <para>The returned name is a folder (ServerDirectory) added to the input file name </para>
        /// </summary>
        static private string _GetServerCheckInOutLogFileName(string i_file_name)
        {
            if (m_login_logout_input.ServerDirectory.Equals(@""))
                return i_file_name;
            else
                return m_login_logout_input.ServerDirectory + @"/" + i_file_name;

        } // _GetServerCheckInOutLogFileName


        /// <summary>Returns the file name for the checkin-checkout logfile
        /// <para>1. File name and subdirectory name from the input data object</para>
        /// <para>2. Create the combined full (with path) name for the local file on the exe directory.</para>
        /// </summary>
        static private string _GetCheckInOutLogFileName()
        {
            string local_directory = m_login_logout_input.LocalDirectory;
            string local_address_file = _FileNameWithPath(m_login_logout_input.LogFileName, local_directory, m_login_logout_input.ExeDirectory);

            return local_address_file;
        } // _GetCheckInOutLogFileName


        /// <summary>Get the full name with path for the input file i_file_name.</summary>
        static private string _FileNameWithPath(string i_file_name, string i_directory, string i_exe_directory)
        {
            string path_file_name = Path.Combine(_ExeDirectory(i_directory, i_exe_directory), i_file_name);

            return path_file_name;
        } // _FileNameWithPath

        /// <summary>Get full name to the exe directory. Create the directory if not existing.</summary>
        static private string _ExeDirectory(string i_directory, string i_exe_directory)
        {
            string email_addresses_directory = Path.Combine(i_exe_directory, i_directory);

            if (!Directory.Exists(email_addresses_directory))
            {
                Directory.CreateDirectory(email_addresses_directory);
            }

            return email_addresses_directory;
        } // _ExeDirectory

        /// <summary>Check the input data object</summary>
        static private bool _CheckInput(LoginLogoutInput i_login_logout_input, out string o_error)
        {
            o_error = "";

            if (null == i_login_logout_input)
            {
                o_error = "LoginLogout._CheckInput Programming error: Input object is null (Init has not been called)";
                return false;
            } // CheckInput

            string err_msg_input = @"";
            if (!i_login_logout_input.CheckInput(out err_msg_input))
            {
                o_error = err_msg_input;
                return false;
            }

            return true;

        } // _CheckInput

        #endregion // Private functions

    } // LoginLogout
} // namespace
