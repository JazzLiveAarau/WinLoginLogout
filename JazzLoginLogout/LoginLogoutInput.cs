using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JazzLoginLogout
{
    /// <summary>Input data for the class LoginLogout
    /// <para>Connection, file and folder data must be set</para>
    /// <para>Login-logout file strings, messages and error messages have default strings</para>
    /// </summary>
    public class LoginLogoutInput
    {
        #region Connection data

        /// <summary>FTP host</summary>
        private string m_ftp_host = "www.jazzliveaarau.ch";

        /// <summary>Get or set the FTP host</summary>
        public string FtpHost
        { get { return m_ftp_host; } set { m_ftp_host = value; } }

        /// <summary>FTP user</summary>
        private string m_ftp_user = "appdata@jazzliveaarau.ch";

        /// <summary>Get or set the FTP user</summary>
        public string FtpUser
        { get { return m_ftp_user; } set { m_ftp_user = value; } }

        /// <summary>FTP password for the download and upload of the login/logout file</summary>
        private string m_ftp_password = "";

        /// <summary>Get or set the FTP password</summary>
        public string FtpPassword
        { get { return m_ftp_password; } set { m_ftp_password = value; } }

        #endregion // Connection data

        #region File and folder data

        /// <summary>Name of the checkin-checkout logfile for the data</summary>
        private string m_log_file_name = "CheckInCheckOut.log";

        /// <summary>Get or set the name of the checkin-checkout logfile </summary>
        public string LogFileName
        { get { return m_log_file_name; } set { m_log_file_name = value; } }

        /// <summary>Name of the server directory for the checkin-checkout logfile</summary>
        //QQ private string m_server_directory = @"LoginLogout";
        private string m_server_directory = @"appdata/LoginLogout";

        /// <summary>Get or set the name of the server directory for the checkin-checkout logfile</summary>
        public string ServerDirectory
        { get { return m_server_directory; } set { m_server_directory = value; } }

        /// <summary>Name of the local (exe) directory for the checkin-checkout logfile</summary>
        private string m_local_directory = @"appdata/LoginLogout";

        /// <summary>Get or set the name of the local directory for the checkin-checkout logfile</summary>
        public string LocalDirectory
        { get { return m_local_directory; } set { m_local_directory = value; } }

        /// <summary>Path to the exe directory.</summary>
        private string m_exe_directory = @"";

        /// <summary>Get or set the name of the exe directory</summary>
        public string ExeDirectory
        { get { return m_exe_directory; } set { m_exe_directory = value; } }


        #endregion // File and folder data

        #region Login-logout strings

        /// <summary>Start string for a checkin row in logfile</summary>
        private string m_checkin_start = @"Checked in by:";

        /// <summary>Get or set the start string for a checkin row in logfile</summary>
        public string CheckInStart
        { get { return m_checkin_start; } set { m_checkin_start = value; } }

        /// <summary>Start string for a checkout row in logfile</summary>
        private string m_checkout_start = @"Checked out by:";

        /// <summary>Get or set the start string for a checkout row in logfile</summary>
        public string CheckOutStart
        { get { return m_checkout_start; } set { m_checkout_start = value; } }

        #endregion // Login-logout strings

        #region Messages

        /// <summary>Message: Data are checked out</summary>
        private string m_msg_data_are_checked_out = @"Daten sind ausgecheckt";

        /// <summary>Get or set the error message: Data have been checked out </summary>
        public string MsgDataAreCheckedOut
        { get { return m_msg_data_are_checked_out; } set { m_msg_data_are_checked_out = value; } }

        /// <summary>Message: Checkout of data was forced</summary>
        private string m_msg_data_are_forced_checked_out = @"Daten waren schon ausgecheckt, aber wurde wieder ausgechecked";

        /// <summary>Get or set the error message: Checkout of data was forced</summary>
        public string MsgDataAreForcedCheckedOut
        { get { return m_msg_data_are_forced_checked_out; } set { m_msg_data_are_forced_checked_out = value; } }

        /// <summary>Message: Data are checked in</summary>
        private string m_msg_data_are_checked_in = @"Daten sind eingecheckt";

        /// <summary>Set or get message: Data are checked in</summary>
        public string MsgDataAreCheckedIn
        { get { return m_msg_data_are_checked_in; } set { m_msg_data_are_checked_in = value; } }

        /// <summary>Message: Force checkout anyhow? </summary>
        private string m_msg_force_checkout = "Trotzdem check out?";

        /// <summary>Set or get message: Force checkout anyhow?</summary>
        public string MsgForceCheckOut
        { get { return m_msg_force_checkout; } set { m_msg_force_checkout = value; } }

        #endregion // Messages

        #region Error messages

        /// <summary>Error message: No connection to Internet is available</summary>
        private string m_error_msg_no_internet_connection = @"Keine Verbindung zu Internet ist vorhanden";

        /// <summary>Get or set the error message: No connection to Internet is available </summary>
        public string ErrMsgNoInternetConnection
        { get { return m_error_msg_no_internet_connection; } set { m_error_msg_no_internet_connection = value; } }

        /// <summary>Error message: Data have not been checked out </summary>
        private string m_error_msg_data_not_checked_out = @"Daten sind nicht ausgecheckt";

        /// <summary>Get or set the error message: Data have not been checked out </summary>
        public string ErrMsgDataNotCheckedOut
        { get { return m_error_msg_data_not_checked_out; } set { m_error_msg_data_not_checked_out = value; } }

        /// <summary>Error message: Failure downloading login-logout file </summary>
        private string m_error_msg_failure_downloading_log = @"Checkin Log-Datei nicht heruntergeladen";

        /// <summary>Get or set the error message: Failure downloading login-logout file </summary>
        public string ErrMsgNoCheckInOutLogFileDownload
        { get { return m_error_msg_failure_downloading_log; } set { m_error_msg_failure_downloading_log = value; } }

        /// <summary>Error message: Data is already checked out by somebody else </summary>
        private string m_error_msg_checked_out_by = @"Adressen sind schon checked out bei ";

        /// <summary>Get or set the error message: Data is already checked out by somebody else </summary>
        public string ErrMsgCheckedOutBy
        { get { return m_error_msg_checked_out_by; } set { m_error_msg_checked_out_by = value; } }


        /// <summary>Error message: Failure uploading the login-logout file </summary>
        private string m_error_msg_upload_logfile = @"Checkin-Checkout Logfile konnte nicht hochgeladen werden";

        /// <summary>Get or set the error message: Failure uploading the login-logout file </summary>
        public string ErrMsgUploadLogfile
        { get { return m_error_msg_upload_logfile; } set { m_error_msg_upload_logfile = value; } }


        #endregion // Error messages

        /// <summary>Check the input data</summary>
        public bool CheckInput(out string o_error)
        {
            bool ret_check = true;
            o_error = @"";

            if (m_exe_directory.Trim().Equals(@""))
            {
                o_error = @"LoginLogoutInput.Check Programming error: Exe directory is not set";
                return false;
            }

            return ret_check;
        } // CheckInput

    } // LoginLogoutInput
} // namespace
