using System.Collections.Generic;

public class BuildMapHelper
{
    #region BuildMap
    public static Dictionary<string, Dictionary<string, string>> BuildMap()
    {
        var tableMap = new Dictionary<string, Dictionary<string, string>>();

        #region Build map for BUSINESS_TYPE
        var tableMapForBUSINESS_TYPE = new Dictionary<string, string>();
        tableMapForBUSINESS_TYPE["id"] = "int";
        tableMapForBUSINESS_TYPE["title"] = "varchar";
        tableMapForBUSINESS_TYPE["userid"] = "varchar";
        tableMap["BUSINESS_TYPE"] = tableMapForBUSINESS_TYPE;
        #endregion

        #region Build map for CALENDAR
        var tableMapForCALENDAR = new Dictionary<string, string>();
        tableMapForCALENDAR["contactid"] = "int";
        tableMapForCALENDAR["createdat"] = "timestamp";
        tableMapForCALENDAR["day"] = "timestamp";
        tableMapForCALENDAR["email"] = "varchar";
        tableMapForCALENDAR["fullname"] = "varchar";
        tableMapForCALENDAR["hour"] = "int";
        tableMapForCALENDAR["id"] = "int";
        tableMapForCALENDAR["phone"] = "varchar";
        tableMapForCALENDAR["productid"] = "int";
        tableMapForCALENDAR["staffid"] = "int";
        tableMapForCALENDAR["status"] = "int";
        tableMapForCALENDAR["storeid"] = "int";
        tableMapForCALENDAR["userid"] = "varchar";
        tableMap["CALENDAR"] = tableMapForCALENDAR;
        #endregion

        #region Build map for CATEGORY_REPORT
        var tableMapForCATEGORY_REPORT = new Dictionary<string, string>();
        tableMapForCATEGORY_REPORT["categorylistcustom"] = "text";
        tableMapForCATEGORY_REPORT["categorylisttype"] = "int";
        tableMapForCATEGORY_REPORT["createdat"] = "timestamp";
        tableMapForCATEGORY_REPORT["datefrom"] = "timestamp";
        tableMapForCATEGORY_REPORT["dateto"] = "timestamp";
        tableMapForCATEGORY_REPORT["datetype"] = "int";
        tableMapForCATEGORY_REPORT["id"] = "int";
        tableMapForCATEGORY_REPORT["ignorecategory"] = "int";
        tableMapForCATEGORY_REPORT["ignoredcategories"] = "text";
        tableMapForCATEGORY_REPORT["modifiedat"] = "timestamp";
        tableMapForCATEGORY_REPORT["name"] = "varchar";
        tableMapForCATEGORY_REPORT["userid"] = "int";
        tableMap["CATEGORY_REPORT"] = tableMapForCATEGORY_REPORT;
        #endregion

        #region Build map for CONTACT
        var tableMapForCONTACT = new Dictionary<string, string>();
        tableMapForCONTACT["address"] = "varchar";
        tableMapForCONTACT["avatarurl"] = "varchar";
        tableMapForCONTACT["businesstypeid"] = "int";
        tableMapForCONTACT["buycount"] = "int";
        tableMapForCONTACT["code"] = "varchar";
        tableMapForCONTACT["dateofbirth"] = "timestamp";
        tableMapForCONTACT["email"] = "varchar";
        tableMapForCONTACT["fullname"] = "varchar";
        tableMapForCONTACT["gender"] = "varchar";
        tableMapForCONTACT["id"] = "int";
        tableMapForCONTACT["isimportant"] = "bit";
        tableMapForCONTACT["lastaction"] = "varchar";
        tableMapForCONTACT["lastactive"] = "timestamp";
        tableMapForCONTACT["levelid"] = "int";
        tableMapForCONTACT["mobile"] = "varchar";
        tableMapForCONTACT["note"] = "text";
        tableMapForCONTACT["point"] = "decimal";
        tableMapForCONTACT["saleslineid"] = "int";
        tableMapForCONTACT["staffid"] = "int";
        tableMapForCONTACT["userid"] = "varchar";
        tableMapForCONTACT["fbuserid"] = "varchar";
        tableMapForCONTACT["source"] = "varchar";
        tableMap["CONTACT"] = tableMapForCONTACT;
        #endregion

        #region Build map for CUSTOMER_DISCOUNT
        var tableMapForCUSTOMER_DISCOUNT = new Dictionary<string, string>();
        tableMapForCUSTOMER_DISCOUNT["categoryid"] = "int";
        tableMapForCUSTOMER_DISCOUNT["collaboratorid"] = "int";
        tableMapForCUSTOMER_DISCOUNT["conditionquantity"] = "int";
        tableMapForCUSTOMER_DISCOUNT["contactid"] = "int";
        tableMapForCUSTOMER_DISCOUNT["createdat"] = "timestamp";
        tableMapForCUSTOMER_DISCOUNT["discountvalue"] = "decimal";
        tableMapForCUSTOMER_DISCOUNT["id"] = "int";
        tableMapForCUSTOMER_DISCOUNT["iscollaboratorprice"] = "bit";
        tableMapForCUSTOMER_DISCOUNT["productid"] = "int";
        tableMapForCUSTOMER_DISCOUNT["type"] = "int";
        tableMapForCUSTOMER_DISCOUNT["userid"] = "varchar";
        tableMap["CUSTOMER_DISCOUNT"] = tableMapForCUSTOMER_DISCOUNT;
        #endregion

        #region Build map for CUSTOMER_PRICE
        var tableMapForCUSTOMER_PRICE = new Dictionary<string, string>();
        tableMapForCUSTOMER_PRICE["categoryid"] = "int";
        tableMapForCUSTOMER_PRICE["collaboratorid"] = "int";
        tableMapForCUSTOMER_PRICE["contactid"] = "int";
        tableMapForCUSTOMER_PRICE["createdat"] = "timestamp";
        tableMapForCUSTOMER_PRICE["id"] = "int";
        tableMapForCUSTOMER_PRICE["iscollaboratorprice"] = "bit";
        tableMapForCUSTOMER_PRICE["price"] = "decimal";
        tableMapForCUSTOMER_PRICE["productid"] = "int";
        tableMapForCUSTOMER_PRICE["userid"] = "varchar";
        tableMap["CUSTOMER_PRICE"] = tableMapForCUSTOMER_PRICE;
        #endregion

        #region Build map for DEBT
        var tableMapForDEBT = new Dictionary<string, string>();
        tableMapForDEBT["code"] = "varchar";
        tableMapForDEBT["contactid"] = "int";
        tableMapForDEBT["countpaid"] = "int";
        tableMapForDEBT["createdat"] = "timestamp";
        tableMapForDEBT["debttype"] = "int";
        tableMapForDEBT["id"] = "int";
        tableMapForDEBT["interestrate"] = "int";
        tableMapForDEBT["ispaid"] = "bit";
        tableMapForDEBT["ispurchase"] = "bit";
        tableMapForDEBT["maturitydate"] = "timestamp";
        tableMapForDEBT["modifiedat"] = "timestamp";
        tableMapForDEBT["note"] = "varchar";
        tableMapForDEBT["orderid"] = "int";
        tableMapForDEBT["productcount"] = "int";
        tableMapForDEBT["productid"] = "int";
        tableMapForDEBT["receivednoteid"] = "int";
        tableMapForDEBT["staffid"] = "int";
        tableMapForDEBT["storeid"] = "int";
        tableMapForDEBT["userid"] = "varchar";
        tableMapForDEBT["value"] = "decimal";
        tableMapForDEBT["valuepaid"] = "decimal";
        tableMap["DEBT"] = tableMapForDEBT;
        #endregion

        #region Build map for DEBT_REPORT
        var tableMapForDEBT_REPORT = new Dictionary<string, string>();
        tableMapForDEBT_REPORT["agefrom"] = "int";
        tableMapForDEBT_REPORT["ageto"] = "int";
        tableMapForDEBT_REPORT["agetype"] = "int";
        tableMapForDEBT_REPORT["createdat"] = "timestamp";
        tableMapForDEBT_REPORT["customlist"] = "text";
        tableMapForDEBT_REPORT["customlisttype"] = "int";
        tableMapForDEBT_REPORT["datefrom"] = "timestamp";
        tableMapForDEBT_REPORT["dateto"] = "timestamp";
        tableMapForDEBT_REPORT["datetype"] = "int";
        tableMapForDEBT_REPORT["debttype"] = "int";
        tableMapForDEBT_REPORT["gendertype"] = "int";
        tableMapForDEBT_REPORT["id"] = "int";
        tableMapForDEBT_REPORT["ignore"] = "int";
        tableMapForDEBT_REPORT["ignoredlist"] = "text";
        tableMapForDEBT_REPORT["modifiedat"] = "timestamp";
        tableMapForDEBT_REPORT["name"] = "varchar";
        tableMapForDEBT_REPORT["reporttype"] = "int";
        tableMapForDEBT_REPORT["userid"] = "int";
        tableMap["DEBT_REPORT"] = tableMapForDEBT_REPORT;
        #endregion

        #region Build map for DEBT_TO_CATEGORY
        var tableMapForDEBT_TO_CATEGORY = new Dictionary<string, string>();
        tableMapForDEBT_TO_CATEGORY["categoryid"] = "int";
        tableMapForDEBT_TO_CATEGORY["debtid"] = "int";
        tableMapForDEBT_TO_CATEGORY["id"] = "int";
        tableMapForDEBT_TO_CATEGORY["userid"] = "varchar";
        tableMap["DEBT_TO_CATEGORY"] = tableMapForDEBT_TO_CATEGORY;
        #endregion

        #region Build map for EVENT
        var tableMapForEVENT = new Dictionary<string, string>();
        tableMapForEVENT["createdat"] = "timestamp";
        tableMapForEVENT["eventname"] = "varchar";
        tableMapForEVENT["id"] = "int";
        tableMapForEVENT["params"] = "varchar";
        tableMapForEVENT["userid"] = "varchar";
        tableMap["EVENT"] = tableMapForEVENT;
        #endregion

        #region Build map for FBAUTOORDERCONFIG
        var tableMapForFBAUTOORDERCONFIG = new Dictionary<string, string>();
        tableMapForFBAUTOORDERCONFIG["comment"] = "longtext";
        tableMapForFBAUTOORDERCONFIG["id"] = "int";
        tableMapForFBAUTOORDERCONFIG["isactive"] = "bit";
        tableMapForFBAUTOORDERCONFIG["pageid"] = "varchar";
        tableMapForFBAUTOORDERCONFIG["pagename"] = "varchar";
        tableMapForFBAUTOORDERCONFIG["postid"] = "varchar";
        tableMapForFBAUTOORDERCONFIG["userid"] = "varchar";
        tableMapForFBAUTOORDERCONFIG["applyonpostcomment"] = "bit";
        tableMapForFBAUTOORDERCONFIG["applyonmessage"] = "bit";
        tableMapForFBAUTOORDERCONFIG["applyonlivestream"] = "bit";
        tableMapForFBAUTOORDERCONFIG["replytemplate"] = "longtext";
        tableMap["FBAUTOORDERCONFIG"] = tableMapForFBAUTOORDERCONFIG;
        #endregion

        #region Build map for FBAUTOREPLYCONFIG
        var tableMapForFBAUTOREPLYCONFIG = new Dictionary<string, string>();
        tableMapForFBAUTOREPLYCONFIG["comment"] = "longtext";
        tableMapForFBAUTOREPLYCONFIG["id"] = "int";
        tableMapForFBAUTOREPLYCONFIG["isactive"] = "bit";
        tableMapForFBAUTOREPLYCONFIG["pageid"] = "varchar";
        tableMapForFBAUTOREPLYCONFIG["pagename"] = "varchar";
        tableMapForFBAUTOREPLYCONFIG["postid"] = "varchar";
        tableMapForFBAUTOREPLYCONFIG["userid"] = "varchar";
        tableMapForFBAUTOREPLYCONFIG["applyonpostcomment"] = "bit";
        tableMapForFBAUTOREPLYCONFIG["applyonlivestream"] = "bit";
        tableMap["FBAUTOREPLYCONFIG"] = tableMapForFBAUTOREPLYCONFIG;
        #endregion

        #region Build map for FBCOMMENT
        var tableMapForFBCOMMENT = new Dictionary<string, string>();
        tableMapForFBCOMMENT["comment"] = "longtext";
        tableMapForFBCOMMENT["commentid"] = "varchar";
        tableMapForFBCOMMENT["fromuser"] = "bit";
        tableMapForFBCOMMENT["fromuseravatarurl"] = "varchar";
        tableMapForFBCOMMENT["fromuserid"] = "varchar";
        tableMapForFBCOMMENT["fromusername"] = "varchar";
        tableMapForFBCOMMENT["id"] = "int";
        tableMapForFBCOMMENT["pageid"] = "varchar";
        tableMapForFBCOMMENT["pagename"] = "varchar";
        tableMapForFBCOMMENT["parentid"] = "varchar";
        tableMapForFBCOMMENT["postid"] = "varchar";
        tableMapForFBCOMMENT["timestamp"] = "timestamp";
        tableMapForFBCOMMENT["userid"] = "varchar";
        tableMapForFBCOMMENT["notified"] = "bit";
        tableMapForFBCOMMENT["photourl"] = "varchar";
        tableMapForFBCOMMENT["orderid"] = "int";
        tableMapForFBCOMMENT["livevideoid"] = "varchar";
        tableMap["FBCOMMENT"] = tableMapForFBCOMMENT;
        #endregion

        #region Build map for FBMESSAGE
        var tableMapForFBMESSAGE = new Dictionary<string, string>();
        tableMapForFBMESSAGE["fromuser"] = "bit";
        tableMapForFBMESSAGE["fromuseravatarurl"] = "varchar";
        tableMapForFBMESSAGE["fromuserid"] = "varchar";
        tableMapForFBMESSAGE["fromusername"] = "varchar";
        tableMapForFBMESSAGE["id"] = "int";
        tableMapForFBMESSAGE["message"] = "longtext";
        tableMapForFBMESSAGE["messageid"] = "varchar";
        tableMapForFBMESSAGE["pageid"] = "varchar";
        tableMapForFBMESSAGE["pagename"] = "varchar";
        tableMapForFBMESSAGE["timestamp"] = "timestamp";
        tableMapForFBMESSAGE["userid"] = "varchar";
        tableMapForFBMESSAGE["notified"] = "bit";
        tableMapForFBMESSAGE["photourl"] = "varchar";
        tableMapForFBMESSAGE["orderid"] = "int";
        tableMap["FBMESSAGE"] = tableMapForFBMESSAGE;
        #endregion

        #region Build map for FBMESSAGEFLOW
        var tableMapForFBMESSAGEFLOW = new Dictionary<string, string>();
        tableMapForFBMESSAGEFLOW["fromuser"] = "bit";
        tableMapForFBMESSAGEFLOW["fromuseravatarurl"] = "varchar";
        tableMapForFBMESSAGEFLOW["fromuserid"] = "varchar";
        tableMapForFBMESSAGEFLOW["fromusername"] = "varchar";
        tableMapForFBMESSAGEFLOW["id"] = "int";
        tableMapForFBMESSAGEFLOW["isread"] = "bit";
        tableMapForFBMESSAGEFLOW["lastmessage"] = "longtext";
        tableMapForFBMESSAGEFLOW["lastmessageid"] = "varchar";
        tableMapForFBMESSAGEFLOW["lasttimestamp"] = "timestamp";
        tableMapForFBMESSAGEFLOW["pageid"] = "varchar";
        tableMapForFBMESSAGEFLOW["pagename"] = "varchar";
        tableMapForFBMESSAGEFLOW["userid"] = "varchar";
        tableMapForFBMESSAGEFLOW["photourl"] = "varchar";
        tableMap["FBMESSAGEFLOW"] = tableMapForFBMESSAGEFLOW;
        #endregion

        #region Build map for FBPAGE
        var tableMapForFBPAGE = new Dictionary<string, string>();
        tableMapForFBPAGE["accesstoken"] = "varchar";
        tableMapForFBPAGE["avatarurl"] = "varchar";
        tableMapForFBPAGE["createdat"] = "timestamp";
        tableMapForFBPAGE["id"] = "int";
        tableMapForFBPAGE["isconnected"] = "bit";
        tableMapForFBPAGE["modifiedat"] = "timestamp";
        tableMapForFBPAGE["name"] = "varchar";
        tableMapForFBPAGE["pageid"] = "varchar";
        tableMapForFBPAGE["userid"] = "varchar";
        tableMap["FBPAGE"] = tableMapForFBPAGE;
        #endregion

        #region Build map for FBPOST
        var tableMapForFBPOST = new Dictionary<string, string>();
        tableMapForFBPOST["fromuser"] = "bit";
        tableMapForFBPOST["id"] = "int";
        tableMapForFBPOST["isread"] = "bit";
        tableMapForFBPOST["lastcomment"] = "longtext";
        tableMapForFBPOST["lastcommentid"] = "varchar";
        tableMapForFBPOST["lastfromuseravatarurl"] = "varchar";
        tableMapForFBPOST["lastfromuserid"] = "varchar";
        tableMapForFBPOST["lastfromusername"] = "varchar";
        tableMapForFBPOST["lastparentid"] = "varchar";
        tableMapForFBPOST["lasttimestamp"] = "timestamp";
        tableMapForFBPOST["pageid"] = "varchar";
        tableMapForFBPOST["pagename"] = "varchar";
        tableMapForFBPOST["postid"] = "varchar";
        tableMapForFBPOST["posttitle"] = "longtext";
        tableMapForFBPOST["userid"] = "varchar";
        tableMapForFBPOST["photourl"] = "varchar";
        tableMapForFBPOST["livevideoid"] = "varchar";
        tableMap["FBPOST"] = tableMapForFBPOST;
        #endregion

        #region Build map for FBTOKEN
        var tableMapForFBTOKEN = new Dictionary<string, string>();
        tableMapForFBTOKEN["accesstoken"] = "varchar";
        tableMapForFBTOKEN["createdat"] = "timestamp";
        tableMapForFBTOKEN["expiredin"] = "varchar";
        tableMapForFBTOKEN["fbuserid"] = "varchar";
        tableMapForFBTOKEN["id"] = "int";
        tableMapForFBTOKEN["tokentype"] = "varchar";
        tableMapForFBTOKEN["userid"] = "varchar";
        tableMap["FBTOKEN"] = tableMapForFBTOKEN;
        #endregion

        #region Build map for FBWEBHOOKMESSAGE
        var tableMapForFBWEBHOOKMESSAGE = new Dictionary<string, string>();
        tableMapForFBWEBHOOKMESSAGE["createdat"] = "timestamp";
        tableMapForFBWEBHOOKMESSAGE["id"] = "int";
        tableMapForFBWEBHOOKMESSAGE["message"] = "longtext";
        tableMap["FBWEBHOOKMESSAGE"] = tableMapForFBWEBHOOKMESSAGE;
        #endregion

        #region Build map for FCMTOKEN
        var tableMapForFCMTOKEN = new Dictionary<string, string>();
        tableMapForFCMTOKEN["createdat"] = "timestamp";
        tableMapForFCMTOKEN["id"] = "int";
        tableMapForFCMTOKEN["token"] = "varchar";
        tableMapForFCMTOKEN["userid"] = "varchar";
        tableMap["FCMTOKEN"] = tableMapForFCMTOKEN;
        #endregion

        #region Build map for LEVEL_CONFIG
        var tableMapForLEVEL_CONFIG = new Dictionary<string, string>();
        tableMapForLEVEL_CONFIG["buycount"] = "int";
        tableMapForLEVEL_CONFIG["id"] = "int";
        tableMapForLEVEL_CONFIG["point"] = "decimal";
        tableMapForLEVEL_CONFIG["title"] = "varchar";
        tableMapForLEVEL_CONFIG["userid"] = "varchar";
        tableMap["LEVEL_CONFIG"] = tableMapForLEVEL_CONFIG;
        #endregion

        #region Build map for MONEY_ACCOUNT
        var tableMapForMONEY_ACCOUNT = new Dictionary<string, string>();
        tableMapForMONEY_ACCOUNT["accountname"] = "varchar";
        tableMapForMONEY_ACCOUNT["bankaccountname"] = "varchar";
        tableMapForMONEY_ACCOUNT["bankname"] = "varchar";
        tableMapForMONEY_ACCOUNT["banknumber"] = "varchar";
        tableMapForMONEY_ACCOUNT["createdat"] = "timestamp";
        tableMapForMONEY_ACCOUNT["defaultstoreid"] = "int";
        tableMapForMONEY_ACCOUNT["id"] = "int";
        tableMapForMONEY_ACCOUNT["isdefault"] = "bit";
        tableMapForMONEY_ACCOUNT["modifiedat"] = "timestamp";
        tableMapForMONEY_ACCOUNT["total"] = "decimal";
        tableMapForMONEY_ACCOUNT["userid"] = "varchar";
        tableMap["MONEY_ACCOUNT"] = tableMapForMONEY_ACCOUNT;
        #endregion

        #region Build map for MONEY_ACCOUNT_TRANSACTION
        var tableMapForMONEY_ACCOUNT_TRANSACTION = new Dictionary<string, string>();
        tableMapForMONEY_ACCOUNT_TRANSACTION["createdat"] = "timestamp";
        tableMapForMONEY_ACCOUNT_TRANSACTION["id"] = "int";
        tableMapForMONEY_ACCOUNT_TRANSACTION["moneyaccountid"] = "int";
        tableMapForMONEY_ACCOUNT_TRANSACTION["note"] = "varchar";
        tableMapForMONEY_ACCOUNT_TRANSACTION["orderid"] = "int";
        tableMapForMONEY_ACCOUNT_TRANSACTION["tradeid"] = "int";
        tableMapForMONEY_ACCOUNT_TRANSACTION["transferfee"] = "decimal";
        tableMapForMONEY_ACCOUNT_TRANSACTION["userid"] = "varchar";
        tableMapForMONEY_ACCOUNT_TRANSACTION["value"] = "decimal";
        tableMap["MONEY_ACCOUNT_TRANSACTION"] = tableMapForMONEY_ACCOUNT_TRANSACTION;
        #endregion

        #region Build map for NOTE
        var tableMapForNOTE = new Dictionary<string, string>();
        tableMapForNOTE["avatarurl"] = "varchar";
        tableMapForNOTE["contactid"] = "int";
        tableMapForNOTE["content"] = "text";
        tableMapForNOTE["createdat"] = "timestamp";
        tableMapForNOTE["frequency"] = "bit";
        tableMapForNOTE["id"] = "int";
        tableMapForNOTE["important"] = "bit";
        tableMapForNOTE["modifiedat"] = "timestamp";
        tableMapForNOTE["userid"] = "varchar";
        tableMapForNOTE["staffid"] = "int";
        tableMap["NOTE"] = tableMapForNOTE;
        #endregion

        #region Build map for NOTE_PICTURE
        var tableMapForNOTE_PICTURE = new Dictionary<string, string>();
        tableMapForNOTE_PICTURE["id"] = "int";
        tableMapForNOTE_PICTURE["imageurl"] = "varchar";
        tableMapForNOTE_PICTURE["noteid"] = "int";
        tableMapForNOTE_PICTURE["userid"] = "varchar";
        tableMapForNOTE_PICTURE["staffid"] = "int";
        tableMap["NOTE_PICTURE"] = tableMapForNOTE_PICTURE;
        #endregion

        #region Build map for ORDER
        var tableMapForORDER = new Dictionary<string, string>();
        tableMapForORDER["amountfrompoint"] = "decimal";
        tableMapForORDER["billofladingcode"] = "varchar";
        tableMapForORDER["change"] = "decimal";
        tableMapForORDER["contactaddress"] = "varchar";
        tableMapForORDER["contactid"] = "int";
        tableMapForORDER["contactname"] = "varchar";
        tableMapForORDER["contactphone"] = "varchar";
        tableMapForORDER["createdat"] = "timestamp";
        tableMapForORDER["deliveryaddress"] = "varchar";
        tableMapForORDER["discount"] = "decimal";
        tableMapForORDER["discountontotal"] = "decimal";
        tableMapForORDER["hasshipinfo"] = "bit";
        tableMapForORDER["id"] = "int";
        tableMapForORDER["itemsjson"] = "text";
        tableMapForORDER["moneyaccountid"] = "int";
        tableMapForORDER["netvalue"] = "decimal";
        tableMapForORDER["note"] = "text";
        tableMapForORDER["ordercode"] = "varchar";
        tableMapForORDER["paid"] = "decimal";
        tableMapForORDER["pointamount"] = "decimal";
        tableMapForORDER["pointpaymentexchange"] = "decimal";
        tableMapForORDER["shipperid"] = "int";
        tableMapForORDER["shippername"] = "varchar";
        tableMapForORDER["shipperphone"] = "varchar";
        tableMapForORDER["shippingfee"] = "decimal";
        tableMapForORDER["shippingpartner"] = "varchar";
        tableMapForORDER["staffid"] = "int";
        tableMapForORDER["status"] = "int";
        tableMapForORDER["storeid"] = "int";
        tableMapForORDER["tableid"] = "int";
        tableMapForORDER["tax"] = "decimal";
        tableMapForORDER["taxtype"] = "int";
        tableMapForORDER["total"] = "decimal";
        tableMapForORDER["usepointpayment"] = "bit";
        tableMapForORDER["shipcostoncustomer"] = "bit";
        tableMapForORDER["userid"] = "varchar";
        tableMap["ORDER"] = tableMapForORDER;
        #endregion

        #region Build map for POINT_CONFIG
        var tableMapForPOINT_CONFIG = new Dictionary<string, string>();
        tableMapForPOINT_CONFIG["allowpayment"] = "bit";
        tableMapForPOINT_CONFIG["categoryid"] = "int";
        tableMapForPOINT_CONFIG["contactid"] = "int";
        tableMapForPOINT_CONFIG["exchange"] = "decimal";
        tableMapForPOINT_CONFIG["forallcustomer"] = "bit";
        tableMapForPOINT_CONFIG["id"] = "int";
        tableMapForPOINT_CONFIG["paymentafterbuycount"] = "int";
        tableMapForPOINT_CONFIG["paymentexchange"] = "decimal";
        tableMapForPOINT_CONFIG["productid"] = "int";
        tableMapForPOINT_CONFIG["userid"] = "varchar";
        tableMap["POINT_CONFIG"] = tableMapForPOINT_CONFIG;
        #endregion

        #region Build map for POINT_HISTORY
        var tableMapForPOINT_HISTORY = new Dictionary<string, string>();
        tableMapForPOINT_HISTORY["amount"] = "decimal";
        tableMapForPOINT_HISTORY["contactid"] = "int";
        tableMapForPOINT_HISTORY["createdat"] = "timestamp";
        tableMapForPOINT_HISTORY["id"] = "int";
        tableMapForPOINT_HISTORY["orderid"] = "int";
        tableMapForPOINT_HISTORY["pointconfigid"] = "int";
        tableMapForPOINT_HISTORY["userid"] = "varchar";
        tableMap["POINT_HISTORY"] = tableMapForPOINT_HISTORY;
        #endregion

        #region Build map for PRODUCT
        var tableMapForPRODUCT = new Dictionary<string, string>();
        tableMapForPRODUCT["avatarurl"] = "varchar";
        tableMapForPRODUCT["barcode"] = "varchar";
        tableMapForPRODUCT["code"] = "varchar";
        tableMapForPRODUCT["collaboratorprice"] = "decimal";
        tableMapForPRODUCT["costprice"] = "decimal";
        tableMapForPRODUCT["costpriceforeign"] = "decimal";
        tableMapForPRODUCT["count"] = "decimal";
        tableMapForPRODUCT["createdat"] = "timestamp";
        tableMapForPRODUCT["description"] = "text";
        tableMapForPRODUCT["expiredat"] = "timestamp";
        tableMapForPRODUCT["foreigncurrency"] = "varchar";
        tableMapForPRODUCT["hidefromlist"] = "bit";
        tableMapForPRODUCT["id"] = "int";
        tableMapForPRODUCT["imageurlsjson"] = "text";
        tableMapForPRODUCT["iscombo"] = "bit";
        tableMapForPRODUCT["ishotproduct"] = "bit";
        tableMapForPRODUCT["ismaterial"] = "bit";
        tableMapForPRODUCT["isnewproduct"] = "bit";
        tableMapForPRODUCT["isoption"] = "bit";
        tableMapForPRODUCT["ispublic"] = "bit";
        tableMapForPRODUCT["issale"] = "bit";
        tableMapForPRODUCT["isservice"] = "bit";
        tableMapForPRODUCT["itemsjson"] = "text";
        tableMapForPRODUCT["materialsjson"] = "text";
        tableMapForPRODUCT["modifiedat"] = "timestamp";
        tableMapForPRODUCT["originalprice"] = "decimal";
        tableMapForPRODUCT["price"] = "decimal";
        tableMapForPRODUCT["productcategoryid"] = "int";
        tableMapForPRODUCT["showonweb"] = "bit";
        tableMapForPRODUCT["showpriceonweb"] = "bit";
        tableMapForPRODUCT["status"] = "int";
        tableMapForPRODUCT["step"] = "decimal";
        tableMapForPRODUCT["title"] = "text";
        tableMapForPRODUCT["unit"] = "varchar";
        tableMapForPRODUCT["unitsjson"] = "text";
        tableMapForPRODUCT["userid"] = "varchar";
        tableMap["PRODUCT"] = tableMapForPRODUCT;
        #endregion

        #region Build map for PRODUCT_ATTRIBUTE
        var tableMapForPRODUCT_ATTRIBUTE = new Dictionary<string, string>();
        tableMapForPRODUCT_ATTRIBUTE["id"] = "int";
        tableMapForPRODUCT_ATTRIBUTE["isaddedtoprice"] = "bit";
        tableMapForPRODUCT_ATTRIBUTE["orderindex"] = "int";
        tableMapForPRODUCT_ATTRIBUTE["price"] = "decimal";
        tableMapForPRODUCT_ATTRIBUTE["productid"] = "int";
        tableMapForPRODUCT_ATTRIBUTE["producttypeid"] = "int";
        tableMapForPRODUCT_ATTRIBUTE["selectonly"] = "bit";
        tableMapForPRODUCT_ATTRIBUTE["title"] = "varchar";
        tableMapForPRODUCT_ATTRIBUTE["userid"] = "varchar";
        tableMap["PRODUCT_ATTRIBUTE"] = tableMapForPRODUCT_ATTRIBUTE;
        #endregion

        #region Build map for PRODUCT_BARCODE
        var tableMapForPRODUCT_BARCODE = new Dictionary<string, string>();
        tableMapForPRODUCT_BARCODE["barcode"] = "varchar";
        tableMapForPRODUCT_BARCODE["id"] = "int";
        tableMapForPRODUCT_BARCODE["productid"] = "int";
        tableMapForPRODUCT_BARCODE["unit"] = "varchar";
        tableMapForPRODUCT_BARCODE["userid"] = "varchar";
        tableMap["PRODUCT_BARCODE"] = tableMapForPRODUCT_BARCODE;
        #endregion

        #region Build map for PRODUCT_CATEGORY
        var tableMapForPRODUCT_CATEGORY = new Dictionary<string, string>();
        tableMapForPRODUCT_CATEGORY["createdat"] = "timestamp";
        tableMapForPRODUCT_CATEGORY["description"] = "varchar";
        tableMapForPRODUCT_CATEGORY["id"] = "int";
        tableMapForPRODUCT_CATEGORY["modifiedat"] = "timestamp";
        tableMapForPRODUCT_CATEGORY["name"] = "varchar";
        tableMapForPRODUCT_CATEGORY["userid"] = "varchar";
        tableMap["PRODUCT_CATEGORY"] = tableMapForPRODUCT_CATEGORY;
        #endregion

        #region Build map for PRODUCT_NOTE
        var tableMapForPRODUCT_NOTE = new Dictionary<string, string>();
        tableMapForPRODUCT_NOTE["amount"] = "decimal";
        tableMapForPRODUCT_NOTE["amountforeign"] = "decimal";
        tableMapForPRODUCT_NOTE["basicunit"] = "varchar";
        tableMapForPRODUCT_NOTE["closingstockquantity"] = "decimal";
        tableMapForPRODUCT_NOTE["contactid"] = "int";
        tableMapForPRODUCT_NOTE["createdat"] = "timestamp";
        tableMapForPRODUCT_NOTE["discount"] = "decimal";
        tableMapForPRODUCT_NOTE["discounttype"] = "int";
        tableMapForPRODUCT_NOTE["foreigncurrency"] = "varchar";
        tableMapForPRODUCT_NOTE["id"] = "int";
        tableMapForPRODUCT_NOTE["modifiedat"] = "timestamp";
        tableMapForPRODUCT_NOTE["note"] = "longtext";
        tableMapForPRODUCT_NOTE["openingstockquantity"] = "decimal";
        tableMapForPRODUCT_NOTE["orderid"] = "int";
        tableMapForPRODUCT_NOTE["productcode"] = "varchar";
        tableMapForPRODUCT_NOTE["productid"] = "int";
        tableMapForPRODUCT_NOTE["productname"] = "text";
        tableMapForPRODUCT_NOTE["quantity"] = "decimal";
        tableMapForPRODUCT_NOTE["receiveddate"] = "timestamp";
        tableMapForPRODUCT_NOTE["receivednoteid"] = "int";
        tableMapForPRODUCT_NOTE["storeid"] = "int";
        tableMapForPRODUCT_NOTE["tradeid"] = "int";
        tableMapForPRODUCT_NOTE["transfernoteid"] = "int";
        tableMapForPRODUCT_NOTE["unit"] = "varchar";
        tableMapForPRODUCT_NOTE["unitexchange"] = "decimal";
        tableMapForPRODUCT_NOTE["unitprice"] = "decimal";
        tableMapForPRODUCT_NOTE["unitpriceforeign"] = "decimal";
        tableMapForPRODUCT_NOTE["userid"] = "varchar";
        tableMap["PRODUCT_NOTE"] = tableMapForPRODUCT_NOTE;
        #endregion

        #region Build map for PRODUCT_OPTION
        var tableMapForPRODUCT_OPTION = new Dictionary<string, string>();
        tableMapForPRODUCT_OPTION["id"] = "int";
        tableMapForPRODUCT_OPTION["productid"] = "int";
        tableMapForPRODUCT_OPTION["title"] = "varchar";
        tableMapForPRODUCT_OPTION["userid"] = "varchar";
        tableMap["PRODUCT_OPTION"] = tableMapForPRODUCT_OPTION;
        #endregion

        #region Build map for PRODUCT_QUANTITY
        var tableMapForPRODUCT_QUANTITY = new Dictionary<string, string>();
        tableMapForPRODUCT_QUANTITY["id"] = "int";
        tableMapForPRODUCT_QUANTITY["productid"] = "int";
        tableMapForPRODUCT_QUANTITY["quantity"] = "decimal";
        tableMapForPRODUCT_QUANTITY["storeid"] = "int";
        tableMapForPRODUCT_QUANTITY["userid"] = "varchar";
        tableMap["PRODUCT_QUANTITY"] = tableMapForPRODUCT_QUANTITY;
        #endregion

        #region Build map for PRODUCT_REPORT
        var tableMapForPRODUCT_REPORT = new Dictionary<string, string>();
        tableMapForPRODUCT_REPORT["createdat"] = "timestamp";
        tableMapForPRODUCT_REPORT["datefrom"] = "timestamp";
        tableMapForPRODUCT_REPORT["dateto"] = "timestamp";
        tableMapForPRODUCT_REPORT["datetype"] = "int";
        tableMapForPRODUCT_REPORT["id"] = "int";
        tableMapForPRODUCT_REPORT["ignoredproducts"] = "text";
        tableMapForPRODUCT_REPORT["ignoreproduct"] = "int";
        tableMapForPRODUCT_REPORT["modifiedat"] = "timestamp";
        tableMapForPRODUCT_REPORT["name"] = "varchar";
        tableMapForPRODUCT_REPORT["productlistcustom"] = "text";
        tableMapForPRODUCT_REPORT["productlisttype"] = "int";
        tableMapForPRODUCT_REPORT["userid"] = "int";
        tableMap["PRODUCT_REPORT"] = tableMapForPRODUCT_REPORT;
        #endregion

        #region Build map for PRODUCT_TYPE
        var tableMapForPRODUCT_TYPE = new Dictionary<string, string>();
        tableMapForPRODUCT_TYPE["createdat"] = "timestamp";
        tableMapForPRODUCT_TYPE["id"] = "int";
        tableMapForPRODUCT_TYPE["multichoice"] = "bit";
        tableMapForPRODUCT_TYPE["orderindex"] = "int";
        tableMapForPRODUCT_TYPE["productid"] = "int";
        tableMapForPRODUCT_TYPE["title"] = "varchar";
        tableMapForPRODUCT_TYPE["userid"] = "varchar";
        tableMap["PRODUCT_TYPE"] = tableMapForPRODUCT_TYPE;
        #endregion

        #region Build map for PROMOTION
        var tableMapForPROMOTION = new Dictionary<string, string>();
        tableMapForPROMOTION["categoryid"] = "int";
        tableMapForPROMOTION["code"] = "varchar";
        tableMapForPROMOTION["conditiontotalamount"] = "decimal";
        tableMapForPROMOTION["conditiontotalquantity"] = "int";
        tableMapForPROMOTION["contactid"] = "int";
        tableMapForPROMOTION["enddate"] = "timestamp";
        tableMapForPROMOTION["forallcustomer"] = "bit";
        tableMapForPROMOTION["id"] = "int";
        tableMapForPROMOTION["isactive"] = "bit";
        tableMapForPROMOTION["ispercent"] = "bit";
        tableMapForPROMOTION["limitamount"] = "decimal";
        tableMapForPROMOTION["limitquantity"] = "int";
        tableMapForPROMOTION["maxpromotionquantity"] = "int";
        tableMapForPROMOTION["name"] = "varchar";
        tableMapForPROMOTION["productid"] = "int";
        tableMapForPROMOTION["promotioncategoryid"] = "int";
        tableMapForPROMOTION["promotionmaxvalue"] = "decimal";
        tableMapForPROMOTION["promotionproductid"] = "int";
        tableMapForPROMOTION["promotionvalue"] = "decimal";
        tableMapForPROMOTION["startdate"] = "timestamp";
        tableMapForPROMOTION["storeid"] = "int";
        tableMapForPROMOTION["userid"] = "varchar";
        tableMapForPROMOTION["createdat"] = "timestamp";
        tableMap["PROMOTION"] = tableMapForPROMOTION;
        #endregion

        #region Build map for PROMOTION_HISTORY
        var tableMapForPROMOTION_HISTORY = new Dictionary<string, string>();
        tableMapForPROMOTION_HISTORY["amount"] = "decimal";
        tableMapForPROMOTION_HISTORY["contactid"] = "int";
        tableMapForPROMOTION_HISTORY["createdat"] = "timestamp";
        tableMapForPROMOTION_HISTORY["id"] = "int";
        tableMapForPROMOTION_HISTORY["orderid"] = "int";
        tableMapForPROMOTION_HISTORY["promotionid"] = "int";
        tableMapForPROMOTION_HISTORY["quantity"] = "int";
        tableMapForPROMOTION_HISTORY["userid"] = "varchar";
        tableMap["PROMOTION_HISTORY"] = tableMapForPROMOTION_HISTORY;
        #endregion


        #region Build map for QUOTE
        var tableMapForQUOTE = new Dictionary<string, string>();
        tableMapForQUOTE["contactid"] = "int";
        tableMapForQUOTE["createdat"] = "timestamp";
        tableMapForQUOTE["discount"] = "decimal";
        tableMapForQUOTE["discountontotal"] = "decimal";
        tableMapForQUOTE["id"] = "int";
        tableMapForQUOTE["itemsjson"] = "text";
        tableMapForQUOTE["netvalue"] = "decimal";
        tableMapForQUOTE["note"] = "text";
        tableMapForQUOTE["staffid"] = "int";
        tableMapForQUOTE["storeid"] = "int";
        tableMapForQUOTE["tax"] = "decimal";
        tableMapForQUOTE["taxtype"] = "int";
        tableMapForQUOTE["total"] = "decimal";
        tableMapForQUOTE["userid"] = "varchar";
        tableMapForQUOTE["name"] = "varchar";
        tableMapForQUOTE["shippingfee"] = "decimal";
        tableMapForQUOTE["shipcostoncustomer"] = "decimal";
        tableMap["QUOTE"] = tableMapForQUOTE;
        #endregion

        #region Build map for RECEIVED_NOTE
        var tableMapForRECEIVED_NOTE = new Dictionary<string, string>();
        tableMapForRECEIVED_NOTE["code"] = "varchar";
        tableMapForRECEIVED_NOTE["contactid"] = "int";
        tableMapForRECEIVED_NOTE["contactname"] = "varchar";
        tableMapForRECEIVED_NOTE["contactphone"] = "varchar";
        tableMapForRECEIVED_NOTE["createdat"] = "timestamp";
        tableMapForRECEIVED_NOTE["deliveryperson"] = "varchar";
        tableMapForRECEIVED_NOTE["discount"] = "decimal";
        tableMapForRECEIVED_NOTE["foreigncurrency"] = "varchar";
        tableMapForRECEIVED_NOTE["id"] = "int";
        tableMapForRECEIVED_NOTE["itemsjson"] = "longtext";
        tableMapForRECEIVED_NOTE["moneyaccountid"] = "int";
        tableMapForRECEIVED_NOTE["netvalue"] = "decimal";
        tableMapForRECEIVED_NOTE["paid"] = "decimal";
        tableMapForRECEIVED_NOTE["receiver"] = "varchar";
        tableMapForRECEIVED_NOTE["shippingfee"] = "decimal";
        tableMapForRECEIVED_NOTE["staffid"] = "int";
        tableMapForRECEIVED_NOTE["storeid"] = "int";
        tableMapForRECEIVED_NOTE["tax"] = "decimal";
        tableMapForRECEIVED_NOTE["taxtype"] = "int";
        tableMapForRECEIVED_NOTE["total"] = "decimal";
        tableMapForRECEIVED_NOTE["totalforeign"] = "decimal";
        tableMapForRECEIVED_NOTE["userid"] = "varchar";
        tableMap["RECEIVED_NOTE"] = tableMapForRECEIVED_NOTE;
        #endregion

        #region Build map for REPORT
        var tableMapForREPORT = new Dictionary<string, string>();
        tableMapForREPORT["agefrom"] = "int";
        tableMapForREPORT["ageto"] = "int";
        tableMapForREPORT["agetype"] = "int";
        tableMapForREPORT["contactlistcustom"] = "text";
        tableMapForREPORT["contactlisttype"] = "int";
        tableMapForREPORT["createdat"] = "timestamp";
        tableMapForREPORT["datefrom"] = "timestamp";
        tableMapForREPORT["dateto"] = "timestamp";
        tableMapForREPORT["datetype"] = "int";
        tableMapForREPORT["gendertype"] = "int";
        tableMapForREPORT["id"] = "int";
        tableMapForREPORT["ignorecontact"] = "int";
        tableMapForREPORT["ignoredcontacts"] = "text";
        tableMapForREPORT["modifiedat"] = "timestamp";
        tableMapForREPORT["name"] = "varchar";
        tableMapForREPORT["userid"] = "varchar";
        tableMap["REPORT"] = tableMapForREPORT;
        #endregion

        #region Build map for SALES_LINE
        var tableMapForSALES_LINE = new Dictionary<string, string>();
        tableMapForSALES_LINE["id"] = "int";
        tableMapForSALES_LINE["title"] = "varchar";
        tableMapForSALES_LINE["userid"] = "varchar";
        tableMap["SALES_LINE"] = tableMapForSALES_LINE;
        #endregion

        #region Build map for SHIFT
        var tableMapForSHIFT = new Dictionary<string, string>();
        tableMapForSHIFT["endtime"] = "time";
        tableMapForSHIFT["id"] = "int";
        tableMapForSHIFT["name"] = "varchar";
        tableMapForSHIFT["starttime"] = "time";
        tableMapForSHIFT["userid"] = "varchar";
        tableMap["SHIFT"] = tableMapForSHIFT;
        #endregion

        #region Build map for SHOP
        var tableMapForSHOP = new Dictionary<string, string>();
        tableMapForSHOP["address"] = "varchar";
        tableMapForSHOP["bankaccountname"] = "varchar";
        tableMapForSHOP["bankaccountnumber"] = "varchar";
        tableMapForSHOP["bankname"] = "varchar";
        tableMapForSHOP["createdat"] = "timestamp";
        tableMapForSHOP["email"] = "varchar";
        tableMapForSHOP["facebook"] = "varchar";
        tableMapForSHOP["facebookid"] = "varchar";
        tableMapForSHOP["iconurl"] = "varchar";
        tableMapForSHOP["id"] = "int";
        tableMapForSHOP["name"] = "varchar";
        tableMapForSHOP["phone"] = "varchar";
        tableMapForSHOP["shortdescription"] = "text";
        tableMapForSHOP["userid"] = "varchar";
        tableMapForSHOP["username"] = "varchar";
        tableMapForSHOP["website"] = "varchar";
        tableMap["SHOP"] = tableMapForSHOP;
        #endregion

        #region Build map for SHOP_CONFIG
        var tableMapForSHOP_CONFIG = new Dictionary<string, string>();
        tableMapForSHOP_CONFIG["allowpointpayment"] = "bit";
        tableMapForSHOP_CONFIG["currency"] = "varchar";
        tableMapForSHOP_CONFIG["currencysymboltotheright"] = "bit";
        tableMapForSHOP_CONFIG["dateformat"] = "varchar";
        tableMapForSHOP_CONFIG["hidecalendarfunction"] = "bit";
        tableMapForSHOP_CONFIG["hidediscountcolumn"] = "bit";
        tableMapForSHOP_CONFIG["hidematerials"] = "bit";
        tableMapForSHOP_CONFIG["hideproductcodeprint"] = "bit";
        tableMapForSHOP_CONFIG["hidetablesfunction"] = "bit";
        tableMapForSHOP_CONFIG["hidetax"] = "bit";
        tableMapForSHOP_CONFIG["id"] = "int";
        tableMapForSHOP_CONFIG["language"] = "varchar";
        tableMapForSHOP_CONFIG["orderinvoicemaxemptyrows"] = "int";
        tableMapForSHOP_CONFIG["outstocknotsell"] = "bit";
        tableMapForSHOP_CONFIG["pointpaymentafterbuycount"] = "int";
        tableMapForSHOP_CONFIG["pointpaymentexchange"] = "decimal";
        tableMapForSHOP_CONFIG["orderprintnote"] = "text";
        tableMapForSHOP_CONFIG["printorderlikeinvoice"] = "bit";
        tableMapForSHOP_CONFIG["showstaffnameundersign"] = "bit";
        tableMapForSHOP_CONFIG["timeformat"] = "varchar";
        tableMapForSHOP_CONFIG["userid"] = "varchar";
        tableMapForSHOP_CONFIG["showstaffphone"] = "bit";
        tableMapForSHOP_CONFIG["hidepromotionfunction"] = "bit";
        tableMap["SHOP_CONFIG"] = tableMapForSHOP_CONFIG;
        #endregion

        #region Build map for STAFF
        var tableMapForSTAFF = new Dictionary<string, string>();
        tableMapForSTAFF["avatarurl"] = "varchar";
        tableMapForSTAFF["cancreatenewtransaction"] = "bit";
        tableMapForSTAFF["cancreateorder"] = "bit";
        tableMapForSTAFF["cancreateupdatedebt"] = "bit";
        tableMapForSTAFF["cancreateupdatenote"] = "bit";
        tableMapForSTAFF["canmanagecontacts"] = "bit";
        tableMapForSTAFF["canupdatedeleteorder"] = "bit";
        tableMapForSTAFF["canupdatedeleteproduct"] = "bit";
        tableMapForSTAFF["canupdatedeletetransaction"] = "bit";
        tableMapForSTAFF["canupdateproductcostprice"] = "bit";
        tableMapForSTAFF["canviewallcontacts"] = "bit";
        tableMapForSTAFF["canviewproductcostprice"] = "bit";
        tableMapForSTAFF["updatestatusexceptdone"] = "bit";
        tableMapForSTAFF["createdat"] = "timestamp";
        tableMapForSTAFF["hasfullaccess"] = "bit";
        tableMapForSTAFF["hourlimit"] = "int";
        tableMapForSTAFF["id"] = "int";
        tableMapForSTAFF["iscollaborator"] = "bit";
        tableMapForSTAFF["name"] = "varchar";
        tableMapForSTAFF["shopname"] = "varchar";
        tableMapForSTAFF["storeid"] = "int";
        tableMapForSTAFF["shiftid"] = "int";
        tableMapForSTAFF["userid"] = "varchar";
        tableMapForSTAFF["username"] = "varchar";
        tableMapForSTAFF["blockviewingquantity"] = "bit";
        tableMapForSTAFF["blockeditingorderprice"] = "bit";
        tableMap["STAFF"] = tableMapForSTAFF;
        #endregion

        #region Build map for STORE
        var tableMapForSTORE = new Dictionary<string, string>();
        tableMapForSTORE["address"] = "varchar";
        tableMapForSTORE["bankaccountname"] = "varchar";
        tableMapForSTORE["bankaccountnumber"] = "varchar";
        tableMapForSTORE["bankname"] = "varchar";
        tableMapForSTORE["createdat"] = "timestamp";
        tableMapForSTORE["email"] = "varchar";
        tableMapForSTORE["facebook"] = "varchar";
        tableMapForSTORE["facebookid"] = "varchar";
        tableMapForSTORE["iconurl"] = "varchar";
        tableMapForSTORE["id"] = "int";
        tableMapForSTORE["moneyaccountid"] = "int";
        tableMapForSTORE["name"] = "varchar";
        tableMapForSTORE["phone"] = "varchar";
        tableMapForSTORE["userid"] = "varchar";
        tableMapForSTORE["website"] = "varchar";
        tableMap["STORE"] = tableMapForSTORE;
        #endregion

        #region Build map for SUBSCRIPTION
        var tableMapForSUBSCRIPTION = new Dictionary<string, string>();
        tableMapForSUBSCRIPTION["enddate"] = "timestamp";
        tableMapForSUBSCRIPTION["id"] = "int";
        tableMapForSUBSCRIPTION["istrial"] = "bit";
        tableMapForSUBSCRIPTION["startdate"] = "timestamp";
        tableMapForSUBSCRIPTION["subscriptiontype"] = "varchar";
        tableMapForSUBSCRIPTION["userid"] = "varchar";
        tableMap["SUBSCRIPTION"] = tableMapForSUBSCRIPTION;
        #endregion

        #region Build map for TABLE
        var tableMapForTABLE = new Dictionary<string, string>();
        tableMapForTABLE["createdat"] = "timestamp";
        tableMapForTABLE["id"] = "int";
        tableMapForTABLE["name"] = "varchar";
        tableMapForTABLE["position"] = "varchar";
        tableMapForTABLE["storeid"] = "int";
        tableMapForTABLE["userid"] = "varchar";
        tableMap["TABLE"] = tableMapForTABLE;
        #endregion

        #region Build map for TICKET
        var tableMapForTICKET = new Dictionary<string, string>();
        tableMapForTICKET["categoryid"] = "int";
        tableMapForTICKET["content"] = "text";
        tableMapForTICKET["createdat"] = "timestamp";
        tableMapForTICKET["email"] = "varchar";
        tableMapForTICKET["id"] = "int";
        tableMapForTICKET["subject"] = "varchar";
        tableMapForTICKET["userid"] = "varchar";
        tableMap["TICKET"] = tableMapForTICKET;
        #endregion

        #region Build map for TRADE
        var tableMapForTRADE = new Dictionary<string, string>();
        tableMapForTRADE["avatarurl"] = "varchar";
        tableMapForTRADE["contactid"] = "int";
        tableMapForTRADE["createdat"] = "timestamp";
        tableMapForTRADE["debtid"] = "int";
        tableMapForTRADE["fee"] = "decimal";
        tableMapForTRADE["id"] = "int";
        tableMapForTRADE["imageurlsjson"] = "text";
        tableMapForTRADE["ispurchase"] = "bit";
        tableMapForTRADE["isreceived"] = "bit";
        tableMapForTRADE["modifiedat"] = "timestamp";
        tableMapForTRADE["moneyaccountid"] = "int";
        tableMapForTRADE["note"] = "varchar";
        tableMapForTRADE["orderid"] = "int";
        tableMapForTRADE["productcount"] = "int";
        tableMapForTRADE["productid"] = "int";
        tableMapForTRADE["receivednoteid"] = "int";
        tableMapForTRADE["saveaccount"] = "bit";
        tableMapForTRADE["staffid"] = "int";
        tableMapForTRADE["transfernoteid"] = "int";
        tableMapForTRADE["userid"] = "varchar";
        tableMapForTRADE["value"] = "decimal";
        tableMap["TRADE"] = tableMapForTRADE;
        #endregion

        #region Build map for TRADE_CATEGORY
        var tableMapForTRADE_CATEGORY = new Dictionary<string, string>();
        tableMapForTRADE_CATEGORY["createdat"] = "timestamp";
        tableMapForTRADE_CATEGORY["id"] = "int";
        tableMapForTRADE_CATEGORY["modifiedat"] = "timestamp";
        tableMapForTRADE_CATEGORY["orderindex"] = "int";
        tableMapForTRADE_CATEGORY["title"] = "varchar";
        tableMapForTRADE_CATEGORY["userid"] = "varchar";
        tableMap["TRADE_CATEGORY"] = tableMapForTRADE_CATEGORY;
        #endregion

        #region Build map for TRADE_TO_CATEGORY
        var tableMapForTRADE_TO_CATEGORY = new Dictionary<string, string>();
        tableMapForTRADE_TO_CATEGORY["categoryid"] = "int";
        tableMapForTRADE_TO_CATEGORY["id"] = "int";
        tableMapForTRADE_TO_CATEGORY["tradeid"] = "int";
        tableMapForTRADE_TO_CATEGORY["userid"] = "varchar";
        tableMap["TRADE_TO_CATEGORY"] = tableMapForTRADE_TO_CATEGORY;
        #endregion

        #region Build map for TRANSFER_NOTE
        var tableMapForTRANSFER_NOTE = new Dictionary<string, string>();
        tableMapForTRANSFER_NOTE["code"] = "varchar";
        tableMapForTRANSFER_NOTE["createdat"] = "timestamp";
        tableMapForTRANSFER_NOTE["deliverer"] = "varchar";
        tableMapForTRANSFER_NOTE["deliveryaddress"] = "text";
        tableMapForTRANSFER_NOTE["exportmoneyaccountid"] = "int";
        tableMapForTRANSFER_NOTE["exportstoreid"] = "int";
        tableMapForTRANSFER_NOTE["haspayment"] = "bit";
        tableMapForTRANSFER_NOTE["id"] = "int";
        tableMapForTRANSFER_NOTE["importmoneyaccountid"] = "int";
        tableMapForTRANSFER_NOTE["importstoreid"] = "int";
        tableMapForTRANSFER_NOTE["itemsjson"] = "longtext";
        tableMapForTRANSFER_NOTE["modifiedat"] = "timestamp";
        tableMapForTRANSFER_NOTE["receiver"] = "varchar";
        tableMapForTRANSFER_NOTE["staffid"] = "int";
        tableMapForTRANSFER_NOTE["transportation"] = "varchar";
        tableMapForTRANSFER_NOTE["userid"] = "varchar";
        tableMap["TRANSFER_NOTE"] = tableMapForTRANSFER_NOTE;
        #endregion

        #region Build map for USER
        var tableMapForUSER = new Dictionary<string, string>();
        tableMapForUSER["avatarurl"] = "varchar";
        tableMapForUSER["facebookaddress"] = "varchar";
        tableMapForUSER["id"] = "binary";
        tableMapForUSER["joineddate"] = "timestamp";
        tableMapForUSER["rank"] = "int";
        tableMapForUSER["rankatacoins"] = "int";
        tableMapForUSER["totalatacoins"] = "int";
        tableMapForUSER["totalcomments"] = "int";
        tableMapForUSER["totallikes"] = "int";
        tableMapForUSER["totalshares"] = "int";
        tableMapForUSER["totalviews"] = "int";
        tableMapForUSER["username"] = "varchar";
        tableMap["USER"] = tableMapForUSER;
        #endregion

        #region Build map for USER_ACTIVITY
        var tableMapForUSER_ACTIVITY = new Dictionary<string, string>();
        tableMapForUSER_ACTIVITY["action"] = "varchar";
        tableMapForUSER_ACTIVITY["createdat"] = "timestamp";
        tableMapForUSER_ACTIVITY["feature"] = "varchar";
        tableMapForUSER_ACTIVITY["id"] = "int";
        tableMapForUSER_ACTIVITY["note"] = "varchar";
        tableMapForUSER_ACTIVITY["userid"] = "varchar";
        tableMapForUSER_ACTIVITY["session"] = "varchar";
        tableMap["USER_ACTIVITY"] = tableMapForUSER_ACTIVITY;
        #endregion

        #region Build map for USER_AGE
        var tableMapForUSER_AGE = new Dictionary<string, string>();
        tableMapForUSER_AGE["age"] = "int";
        tableMapForUSER_AGE["lastdate"] = "date";
        tableMapForUSER_AGE["id"] = "int";
        tableMapForUSER_AGE["userid"] = "varchar";
        tableMap["USER_AGE"] = tableMapForUSER_AGE;
        #endregion

        #region Build map for USER_ONLINE
        var tableMapForUSER_ONLINE = new Dictionary<string, string>();
        tableMapForUSER_ONLINE["createdat"] = "timestamp";
        tableMapForUSER_ONLINE["updatedat"] = "timestamp";
        tableMapForUSER_ONLINE["minutesspent"] = "int";
        tableMapForUSER_ONLINE["createddate"] = "date";
        tableMapForUSER_ONLINE["id"] = "int";
        tableMapForUSER_ONLINE["userid"] = "varchar";
        tableMap["USER_ONLINE"] = tableMapForUSER_ONLINE;
        #endregion

        #region Build map for USER_PROFILE
        var tableMapForUSER_PROFILE = new Dictionary<string, string>();
        tableMapForUSER_PROFILE["avatarurl"] = "varchar";
        tableMapForUSER_PROFILE["birthday"] = "timestamp";
        tableMapForUSER_PROFILE["displayname"] = "varchar";
        tableMapForUSER_PROFILE["email"] = "varchar";
        tableMapForUSER_PROFILE["facebookid"] = "varchar";
        tableMapForUSER_PROFILE["gender"] = "bit";
        tableMapForUSER_PROFILE["googleid"] = "varchar";
        tableMapForUSER_PROFILE["isemailmethod"] = "bit";
        tableMapForUSER_PROFILE["passwordhashed"] = "varchar";
        tableMapForUSER_PROFILE["phonenumber"] = "varchar";
        tableMapForUSER_PROFILE["userid"] = "binary";
        tableMap["USER_PROFILE"] = tableMapForUSER_PROFILE;
        #endregion

        #region Build map for WEB
        var tableMapForWEB = new Dictionary<string, string>();
        tableMapForWEB["aliasurl"] = "varchar";
        tableMapForWEB["bannertext1"] = "varchar";
        tableMapForWEB["bannertext2"] = "varchar";
        tableMapForWEB["bannertext3"] = "varchar";
        tableMapForWEB["bannerurl1"] = "varchar";
        tableMapForWEB["bannerurl2"] = "varchar";
        tableMapForWEB["bannerurl3"] = "varchar";
        tableMapForWEB["color"] = "varchar";
        tableMapForWEB["createdat"] = "timestamp";
        tableMapForWEB["fbfanpageid"] = "varchar";
        tableMapForWEB["fbfanpageurl"] = "varchar";
        tableMapForWEB["id"] = "int";
        tableMapForWEB["introduce"] = "text";
        tableMapForWEB["isenableweb"] = "bit";
        tableMapForWEB["mapiframebig"] = "text";
        tableMapForWEB["mapiframesmall"] = "text";
        tableMapForWEB["openletter"] = "text";
        tableMapForWEB["shopid"] = "int";
        tableMapForWEB["shorttitle"] = "varchar";
        tableMapForWEB["userid"] = "varchar";
        tableMap["WEB"] = tableMapForWEB;
        #endregion

        #region Build map for ONLINE_ORDER
        var tableMapForONLINE_ORDER = new Dictionary<string, string>();
        tableMapForONLINE_ORDER["billofladingcode"] = "varchar";
        tableMapForONLINE_ORDER["change"] = "decimal";
        tableMapForONLINE_ORDER["contactaddress"] = "varchar";
        tableMapForONLINE_ORDER["contactid"] = "int";
        tableMapForONLINE_ORDER["contactname"] = "varchar";
        tableMapForONLINE_ORDER["contactphone"] = "varchar";
        tableMapForONLINE_ORDER["createdat"] = "timestamp";
        tableMapForONLINE_ORDER["deliveryaddress"] = "varchar";
        tableMapForONLINE_ORDER["discount"] = "decimal";
        tableMapForONLINE_ORDER["discountontotal"] = "decimal";
        tableMapForONLINE_ORDER["hasshipinfo"] = "bit";
        tableMapForONLINE_ORDER["id"] = "int";
        tableMapForONLINE_ORDER["itemsjson"] = "text";
        tableMapForONLINE_ORDER["moneyaccountid"] = "int";
        tableMapForONLINE_ORDER["netvalue"] = "decimal";
        tableMapForONLINE_ORDER["note"] = "text";
        tableMapForONLINE_ORDER["ordercode"] = "varchar";
        tableMapForONLINE_ORDER["paid"] = "decimal";
        tableMapForONLINE_ORDER["shipperid"] = "int";
        tableMapForONLINE_ORDER["shippername"] = "varchar";
        tableMapForONLINE_ORDER["shipperphone"] = "varchar";
        tableMapForONLINE_ORDER["shippingfee"] = "decimal";
        tableMapForONLINE_ORDER["shippingpartner"] = "varchar";
        tableMapForONLINE_ORDER["staffid"] = "int";
        tableMapForONLINE_ORDER["status"] = "int";
        tableMapForONLINE_ORDER["storeid"] = "int";
        tableMapForONLINE_ORDER["tableid"] = "int";
        tableMapForONLINE_ORDER["tax"] = "decimal";
        tableMapForONLINE_ORDER["taxtype"] = "int";
        tableMapForONLINE_ORDER["total"] = "decimal";
        tableMapForONLINE_ORDER["userid"] = "varchar";
        tableMapForONLINE_ORDER["convertedorderid"] = "int";
        tableMap["ONLINE_ORDER"] = tableMapForONLINE_ORDER;
        #endregion
        
        #region Build map for ZALOWEBHOOKMESSAGE
        var tableMapForZALOWEBHOOKMESSAGE = new Dictionary<string, string>();
        tableMapForZALOWEBHOOKMESSAGE["createdat"] = "timestamp";
        tableMapForZALOWEBHOOKMESSAGE["id"] = "int";
        tableMapForZALOWEBHOOKMESSAGE["message"] = "longtext";
        tableMap["ZALOWEBHOOKMESSAGE"] = tableMapForZALOWEBHOOKMESSAGE;
        #endregion
        return tableMap;
    }
    #endregion
}