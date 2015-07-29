﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using Uol.PagSeguro.Domain;
using Uol.PagSeguro.Domain.Installment;
using Uol.PagSeguro.Exception;
using Uol.PagSeguro.Log;
using Uol.PagSeguro.Resources;
using Uol.PagSeguro.Util;
using Uol.PagSeguro.XmlParse;

namespace Uol.PagSeguro.Service
{
    public class InstallmentService
    {

        /// <summary>
        /// Request a direct payment session
        /// </summary>
        /// <param name="credentials">PagSeguro credentials</param>
        /// <returns><c cref="T:Uol.PagSeguro.CancelRequestResponse">Result</c></returns>
        public static Installments GetInstallments(String session, Decimal amount, String cardBrand)
        {

            PagSeguroTrace.Info(String.Format(CultureInfo.InvariantCulture, "InstallmentService.GetInstallments() - begin"));
            try
            {
                using (HttpWebResponse response = HttpURLConnectionUtil.GetHttpGetConnection(
                    BuildInstallmentURL(session, amount, cardBrand)))
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {

                        Installments result = new Installments();
                        InstallmentSerializer.Read(reader, result);
                        PagSeguroTrace.Info(String.Format(CultureInfo.InvariantCulture, "InstallmentService.Register({0}) - end", result.ToString()));
                        return result;
                    }
                }
            }
            catch (ArgumentException exception)
            {
                PagSeguroServiceException pse = new PagSeguroServiceException(exception.Message);
                PagSeguroTrace.Error(String.Format(CultureInfo.InvariantCulture, "InstallmentService.Register() - error {0}", exception.Message));
                throw pse;
            }
            catch (WebException exception)
            {
                PagSeguroServiceException pse = HttpURLConnectionUtil.CreatePagSeguroServiceException((HttpWebResponse)exception.Response);
                PagSeguroTrace.Error(String.Format(CultureInfo.InvariantCulture, "InstallmentService.Register() - error {0}", pse));
                throw pse;
            }
        }

        private static String BuildInstallmentURL(String session, Decimal amount, String cardBrand)
        {
            QueryStringBuilder builder = new QueryStringBuilder("{url}?sessionId={session}&amount={amount}&creditCardBrand={cardBrand}");

            builder.ReplaceValue("{url}", PagSeguroConfiguration.InstallmentUri.AbsoluteUri);
            builder.ReplaceValue("{session}", HttpUtility.UrlEncode(session));
            builder.ReplaceValue("{amount}", PagSeguroUtil.DecimalFormat(amount));
            builder.ReplaceValue("{cardBrand}", HttpUtility.UrlEncode(cardBrand));

            return builder.ToString();
        }

    }
}