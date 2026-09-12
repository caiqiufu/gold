using System.Diagnostics;
using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;


namespace AccountsAndPositions
{
    static class ErrorHandler
    {
        /// <summary>
        /// Creates error message.
        /// </summary>
        /// <param name="modName">Module name, which handles an error or in which the error has occured.</param>
        /// <param name="funcName">Function name, which handles an error or in which the error has occured.</param>
        /// <param name="errorDescription">Error description.</param>
        private static string getErrorMessage(string modName, string funcName, string errorDescription)
        {
            return "Module: " + modName + Environment.NewLine +
                   "Function: " + funcName + Environment.NewLine +
                   "Error Description: " + errorDescription;
        }

        /// <summary>
        /// Handles the error and shows a message box.
        /// </summary>
        /// <param name="modName">Module name, in which the error has occurred.</param>
        /// <param name="funcName">Function name, in which the error has occurred.</param>
        /// <param name="ex">The exception generated in the described function of the described module.</param>
        public static void HandleError(string modName, string funcName, Exception ex)
        {
            MessageBox.Show(getErrorMessage(modName, funcName, ex.Message),
                            "AccountsAndPositions", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Handles CQGError and shows a message box.
        /// </summary>
        /// <param name="modName">Module name, which handles an error.</param>
        /// <param name="funcName">Function name, which handles an error.</param>
        /// <param name="ex">CQGError object.</param>
        public static void HandleError(string modName, string funcName, CQG.CQGError error)
        {
            MessageBox.Show(getErrorMessage(modName, funcName, error.Description),
                            "AccountsAndPositions", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
