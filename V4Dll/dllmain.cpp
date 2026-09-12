
#include "pch.h"
#include <iostream>
#include "Winsock2.h"
#include "zlib.h"
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <stdlib.h>
#include "openssl/rsa.h" 
#include "openssl/x509.h"
#include "openssl/pem.h"
#include "openssl/ssl.h"
#include "openssl/err.h"

#include "tinyxml.h"
#include <string.h>
#include <Ws2tcpip.h>

#pragma comment(lib, "Ws2_32.lib")

#if defined(WIN32)||defined(WINCE)||defined(WIN64)
#include <objbase.h>
#else
#include <uuid/uuid.h> 
#endif

#define MAXBUF 1520240
char buffer[MAXBUF] = { 0 };
static char readbuffer[MAXBUF] = { 0 };
static char tem[1520240] = { 0 };
static char UserId[100] = { 0 };
static char SessionId[5] = { 0 };
char nowTime[30] = { 0 };
char szPathName[10] = { 0 };
char strAccountId[50] = { 0 };
std::string uuid;



TiXmlDocument InitDataXML;
TiXmlDocument SettingDataXML;
TiXmlDocument TradingDataXML;


SSL_CTX* ctx;
SSL* ssl;
int sockfd;
struct sockaddr_in dest;


//数据包头1
struct Header
{
	unsigned char A;
	bool bflag;
	unsigned short int nNum1;
	unsigned short int nNum2;
}myHeader;

//数据包头2
struct Header2
{

	unsigned short int nNum1;
	unsigned short int nNum2;
	unsigned short int nNum3;
	unsigned short int nNum4;
}myHeader2;


unsigned short int nSessionLen = 0;



extern "C" __declspec(dllexport)
std::string GetUUID(std::string & strUUID)
{
	strUUID = "";
#if defined(WIN32)||defined(WINCE)||defined(WIN64)
	GUID guid;
	if (!CoCreateGuid(&guid))
	{
		char buffer[64] = { 0 };
		_snprintf_s(buffer, sizeof(buffer),
			//"%08X%04X%04X%02X%02X%02X%02X%02X%02X%02X%02X",    //大写
			"%08x-%04x-%04x-%02x%02x-%02x%02x%02x%02x%02x%02x",        //小写
			guid.Data1, guid.Data2, guid.Data3,
			guid.Data4[0], guid.Data4[1], guid.Data4[2],
			guid.Data4[3], guid.Data4[4], guid.Data4[5],
			guid.Data4[6], guid.Data4[7]);
		strUUID = buffer;
	}
#else
	uuid_t uu;
	uuid_generate(uu);
#endif
	return strUUID;
}

extern "C" __declspec(dllexport)
int getNowTime2(char* nowTime)
{
	char acYear[5] = { 0 };
	char acMonth[5] = { 0 };
	char acDay[5] = { 0 };
	char acHour[5] = { 0 };
	char acMin[5] = { 0 };
	char acSec[5] = { 0 };

	time_t now;
	struct tm* timenow;

	time(&now);
	timenow = localtime(&now);

	strftime(acYear, sizeof(acYear), "%Y", timenow);
	strftime(acMonth, sizeof(acMonth), "%m", timenow);
	strftime(acDay, sizeof(acDay), "%d", timenow);
	strftime(acHour, sizeof(acHour), "%H", timenow);
	strftime(acMin, sizeof(acMin), "%M", timenow);
	strftime(acSec, sizeof(acSec), "%S", timenow);


	//int nYear = atoi(acYear);
	strncat(nowTime, acYear, 4);
	strncat(nowTime, ".", 1);
	strncat(nowTime, acMonth, 2);
	strncat(nowTime, ".", 1);
	strncat(nowTime, acDay, 2);
	strncat(nowTime, " ", 1);
	strncat(nowTime, acHour, 2);
	strncat(nowTime, ":", 1);
	strncat(nowTime, acMin, 2);
	strncat(nowTime, ":", 1);
	strncat(nowTime, acSec, 2);

	return 0;
}

extern "C" __declspec(dllexport)
int getNowTime(char* nowTime)
{
	char acYear[5] = { 0 };
	char acMonth[5] = { 0 };
	char acDay[5] = { 0 };
	char acHour[5] = { 0 };
	char acMin[5] = { 0 };
	char acSec[5] = { 0 };

	time_t now;
	struct tm* timenow;

	time(&now);
	timenow = localtime(&now);

	strftime(acYear, sizeof(acYear), "%Y", timenow);
	strftime(acMonth, sizeof(acMonth), "%m", timenow);
	strftime(acDay, sizeof(acDay), "%d", timenow);
	strftime(acHour, sizeof(acHour), "%H", timenow);
	strftime(acMin, sizeof(acMin), "%M", timenow);
	strftime(acSec, sizeof(acSec), "%S", timenow);

	strncat(nowTime, acYear, 4);
	strncat(nowTime, "-", 1);
	strncat(nowTime, acMonth, 2);
	strncat(nowTime, "-", 1);
	strncat(nowTime, acDay, 2);
	strncat(nowTime, " ", 1);
	strncat(nowTime, acHour, 2);
	strncat(nowTime, ":", 1);
	strncat(nowTime, acMin, 2);
	strncat(nowTime, ":", 1);
	strncat(nowTime, acSec, 2);

	return 0;
}

extern "C" __declspec(dllexport)
void utf8ToGbk(const char* utf8String, char* gbkString)

{

	wchar_t* unicodeStr = NULL;

	int nRetLen = 0;

	nRetLen = MultiByteToWideChar(CP_UTF8, 0, utf8String, -1, NULL, 0);

	//求需求的宽字符数大小

	unicodeStr = (wchar_t*)malloc(nRetLen * sizeof(wchar_t));

	nRetLen = MultiByteToWideChar(CP_UTF8, 0, utf8String, -1, unicodeStr, nRetLen);

	//将utf-8编码转换成unicode编码

	nRetLen = WideCharToMultiByte(CP_ACP, 0, unicodeStr, -1, NULL, 0, NULL, 0);

	//求转换所需字节数

	nRetLen = WideCharToMultiByte(CP_ACP, 0, unicodeStr, -1, gbkString, nRetLen, NULL, 0);

	//unicode编码转换成gbk编码

	free(unicodeStr);

}


void gbkPrint(const char* utf)
{
	char strDesGbk[12500] = { 0 };
	utf8ToGbk(utf, strDesGbk);
	printf("%s", strDesGbk);
}




//extern "C" __declspec(dllexport)
char* WriteF(char* buf, int len)
{
	unsigned int newlen = 0;
	uLongf dlen = 920240;
	uLongf slen = len - 0x26 - 8 - nSessionLen;
	memset(tem, 0, 920240);
	int nRet = uncompress((Bytef*)tem, &dlen, (Bytef*)buf + 8 + nSessionLen + 0x26, slen);
	if (nRet == 0)return tem;
	return buf + 8 + nSessionLen + 0x26;
}


char* readData(char* buffer)
{
	int nRead = 0;
	int len = 0;
	memset(&myHeader, 0, sizeof(myHeader));
	SSL_read(ssl, &myHeader, sizeof(myHeader));
	while (nRead < myHeader.nNum1)
	{
		len = SSL_read(ssl, buffer + nRead, myHeader.nNum1 - nRead);
		nRead += len;

		if (len <= 0) {

			break;
		}
	}
	return WriteF(buffer, nRead);

}





int  writeData(char* buffer, int nLen)
{


	int len = SSL_write(ssl, buffer, nLen);
	if (len > 0) {
		printf("消息发送成功！\n");
	}
	return nLen;
}

void ShowCerts(SSL* ssl)
{
	X509* cert;
	char* line;
	cert = SSL_get_peer_certificate(ssl);
	if (cert != NULL) {
		FILE* fd = fopen("d://a.pem", "wb+");
		PEM_write_X509(fd, cert);
		//i2d_X509_fp(fd, cert);
		fclose(fd);
		printf("数字证书信息:\n");
		line = X509_NAME_oneline(X509_get_subject_name(cert), 0, 0);
		printf("证书: %s\n", line);
		free(line);
		line = X509_NAME_oneline(X509_get_issuer_name(cert), 0, 0);
		printf("颁发者: %s\n", line);
		free(line);
		X509_free(cert);
	}
	else {
		printf("无证书信息！\n");
	}
}



char* GetSessionId(char* buf)
{
	char* p = strstr(buf, "SessionId");
	if (p)
	{

		p += 12;
		char* pEnd = strstr(p, "\"");
		memcpy(SessionId, p, pEnd - p);
		return SessionId;
	}

	return NULL;
}

extern "C" __declspec(dllexport)
char* GetSessionIdX()
{

	return SessionId;
}

extern "C" __declspec(dllexport)
char* GetUserId(char* buf)
{
	char* p = strstr(buf, "UserId");
	if (p)
	{
		memset(UserId, 0, 100);
		p += 9;
		char* pEnd = strstr(p, "\"");
		memcpy(UserId, p, pEnd - p);
		return UserId;
	}

	return NULL;
}

extern "C" __declspec(dllexport)
bool myConnect()
{


	SSL_library_init();
	OpenSSL_add_all_algorithms();
	SSL_load_error_strings();
	ctx = SSL_CTX_new(TLSv1_client_method());//SSLv23_client_method
	if ((sockfd = socket(AF_INET, SOCK_STREAM, 0)) < 0) {
		//	perror("Socket");
		return false;
	}
	//OutputDebugStringA((LPCSTR)"666666666666666");
	struct sockaddr_in dst = dest;
	if (connect(sockfd, (struct sockaddr*)&dest, sizeof(dest)) != 0) {
		//	perror("Connect ");
		return false;

	}

	//OutputDebugStringA((LPCSTR)"777777777777");
	ssl = SSL_new(ctx);
	SSL_set_fd(ssl, sockfd);
	//OutputDebugStringA("8888888888");
	if (SSL_connect(ssl) == -1) {
		unsigned long ulErr = ERR_get_error(); // 获取错误号
		char szErrMsg[1024] = { 0 };
		char szErrGBK[1024] = { 0 };
		char* pTmp = NULL;
		pTmp = ERR_error_string(ulErr, szErrMsg);

		return false;
	}
	return true;
}


extern "C" __declspec(dllexport)
void  myClose()
{
	SSL_shutdown(ssl);
	SSL_free(ssl);
	closesocket(sockfd);
	SSL_CTX_free(ctx);
}



extern "C" __declspec(dllexport)
char* GetSettingData()
{
	char szGetSettingData[255] = { 0 };
	sprintf(szGetSettingData, "{%s}{\"Arguments\":{}, \"Method\":\"GetSettingData\"}\0", GetUUID(uuid).data());
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szGetSettingData) + 8 + nSessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + nSessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szGetSettingData) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, SessionId, nSessionLen);
	memcpy(buffer + 8 + nSessionLen, szGetSettingData, strlen(szGetSettingData));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	char* pRead = readData(readbuffer);
	myClose();
	return pRead;
}



extern "C" __declspec(dllexport)
char* GetTradingData()
{
	if (!myConnect())
	{
		myClose();
		return NULL;
	}
	char szGetTradingData[500] = { 0 };
	sprintf(szGetTradingData, "{%s}{\"Arguments\":{\"ids\":[\"%s\"] }, \"Method\" : \"GetTradingData\"}", GetUUID(uuid).data(), strAccountId);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szGetTradingData) + 8 + nSessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + nSessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szGetTradingData) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, SessionId, nSessionLen);
	memcpy(buffer + 8 + nSessionLen, szGetTradingData, strlen(szGetTradingData));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	char* pRead = readData(readbuffer);
	myClose();
	return pRead;
}

extern "C" __declspec(dllexport)
char* GetAccountId()
{

	return strAccountId;
}
extern "C" __declspec(dllexport)
char* GetInitData()
{
	char szGetInitData[255] = { 0 };
	sprintf(szGetInitData, "{%s}{\"Arguments\":{}, \"Method\":\"GetInitData\"}\0", GetUUID(uuid).data());
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szGetInitData) + 8 + nSessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + nSessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szGetInitData) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, SessionId, nSessionLen);
	memcpy(buffer + 8 + nSessionLen, szGetInitData, strlen(szGetInitData));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	char* pRead = readData(readbuffer);
	if (pRead)
	{
		char* p = strstr(pRead, "Account Id=");
		if (p)
		{
			p += 12;
			char* pEnd = strstr(p, "\"");
			memset(strAccountId, 0, 50);
			memcpy(strAccountId, p, pEnd - p);
		}

	}
	return pRead;
}

extern "C" __declspec(dllexport)
void SetPathName(char* szName)
{
	memset(szPathName, 0, 10);
	strcpy(szPathName, szName);
}

extern "C" __declspec(dllexport)
char* Login(char* szUserName, char* szPass)
{
	//OutputDebugStringA((LPCSTR)"00000000");
	char szLogin2[1200] = { 0 };
	if (!myConnect())
	{
		myClose();
		return NULL;
	}
	//OutputDebugStringA((LPCSTR)"1111111111");
	sprintf(szLogin2, "{%s}{\"Arguments\":{\"appType\":\"18\",\"containsMilSInQuotationTime\":false,\"descriptionInfo\":\"Version:1.0.0.3 System:Operating system version 5.1, corresponds to Windows XP CPU:x86_64\",\"isGuestIdentity\":false,\"language\":\"CHS\",\"loginID\":\"%s\",\"password\":\"%s\",\"sendQuotationInSsl\":false,\"validPathName\":\"%s\"},\"Method\":\"Login\"}\0", GetUUID(uuid).data(), szUserName, szPass, szPathName);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szLogin2) + 8;
	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600;
	myHeader2.nNum3 = strlen(szLogin2) - 0x26;
	myHeader2.nNum4 = 0x0000;
	//OutputDebugStringA((LPCSTR)"2222222222");
	memset(buffer, 0, MAXBUF);
	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, szLogin2, strlen(szLogin2));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	//OutputDebugStringA((LPCSTR)"333333");
	char* pReadBuff = readData(readbuffer);
	if (pReadBuff)
	{
		GetSessionId(pReadBuff);
		nSessionLen = strlen(SessionId);
		GetUserId(pReadBuff);
	}
	//OutputDebugStringA((LPCSTR)"4444444444");
	return pReadBuff;
}



extern "C" __declspec(dllexport)
void  init(char* szIP, int nPort)
{
	WORD ver = MAKEWORD(2, 2);
	WSADATA dat;
	WSAStartup(ver, &dat);
	memset(&dest, 0, sizeof(dest));
	dest.sin_family = AF_INET;
	dest.sin_port = htons(nPort);//4523
	dest.sin_addr.s_addr = inet_addr(szIP);//"203.160.75.182"
	char gbk[100] = { 0 };
	//utf8ToGbk(szIP, gbk);
	OutputDebugString((LPCWSTR)szIP);
	//OutputDebugString((LPCWSTR)gbk);
	itoa(nPort, gbk, 10);
	OutputDebugStringA((LPCSTR)gbk);
	OutputDebugStringA((LPCSTR)"aaaaaaaaaaaaaaaaa");


}


extern "C" __declspec(dllexport)
char* MySell(char* szInstrumentID, char* szLot, char* szPrice)
{
	if (!myConnect())
	{
		myClose();
		return NULL;
	}
	memset(nowTime, 0, 30);
	getNowTime(nowTime);
	char szSell[900] = { 0 };

	sprintf(szSell, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"true\\\" Lot=\\\"%s\\\" IsBuy=\\\"false\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\"/>\\n</Transaction>\\n\"}, \"Method\":\"Place\"}", GetUUID(uuid).data(), szInstrumentID, GetUUID(uuid).data(), nowTime, strAccountId, nowTime, UserId, szLot, szPrice, GetUUID(uuid).data(), szLot);
	//{27b3bacd-99bc-4765-a590-f9f97f39b974}{"Arguments":{"transactionXml":"<Transaction Type=\"0\" OrderType=\"0\" PlacingSettingType=\"0\" AccountID=\"ab9549b4-87b9-403c-b521-4a0da138c352\" InstrumentID=\"0c4188c7-376a-446d-b485-462f37c58536\" EndTime=\"1800-01-01 00:00:00\" ExpireTime=\"\" SubType=\"0\" SubmitTime=\"2021-03-02 08:54:02\" ExpireType=\"0\" SubmitorID=\"186a1d8e-3de8-4b46-b47e-0c109de82f06\"ID=\"b3e7a57a-4f9d-404e-976d-90e3e6492b37\" OrganizationCode=\"\" BeginTime=\"2021-03-02 08:54:02\">\n <Order SetPrice=\"359.62\" SetPrice2=\"\" DQMaxMove=\"0\" IsOpen=\"true\" ID=\"14ada727-fe2a-48f5-9318-3ac819b18d33\" IsBuy=\"true\" TradeOption=\"0\" Lot=\"1\" OriginalLot=\"1\" PhysicalTradeSide=\"0\"/>\n</Transaction>\n"},"Method":"Place"}
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szSell) + 8 + nSessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + nSessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szSell) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, SessionId, nSessionLen);
	memcpy(buffer + 8 + nSessionLen, szSell, strlen(szSell));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	char* pRead = readData(readbuffer);
	myClose();
	return pRead;
}

extern "C" __declspec(dllexport)
char* MyOrderClose(char* szInstrumentID, char* szNowPrice, char* szPrice, char* szLot, char* szOrderID, char* szOrderTime, char* szIsBuy)
{
	if (!myConnect())
	{
		myClose();
		return NULL;
	}
	memset(nowTime, 0, 30);
	getNowTime(nowTime);
	char szBuyAll[1000] = { 0 };
	sprintf(szBuyAll, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"false\\\" Lot=\\\"%s\\\" IsBuy=\\\"%s\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\">\n  <OrderRelation OpenOrderTime=\\\"%s\\\" OpenOrderID=\\\"%s\\\" OpenOrderPrice=\\\"%s\\\" ClosedLot=\\\"%s\\\"/>\n </Order>\\n</Transaction>\\n\"}, \"Method\":\"Place\"}",
		GetUUID(uuid).data(), szInstrumentID, GetUUID(uuid).data(), nowTime, strAccountId, nowTime, UserId, szLot, szPrice, GetUUID(uuid).data(), szLot, szIsBuy, szOrderTime, szOrderID, szNowPrice, szLot);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szBuyAll) + 8 + nSessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + nSessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szBuyAll) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, SessionId, nSessionLen);
	memcpy(buffer + 8 + nSessionLen, szBuyAll, strlen(szBuyAll));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	char* pRead = readData(readbuffer);
	myClose();
	return pRead;

}

extern "C" __declspec(dllexport)
char* MyBuy(char* szInstrumentID, char* szLot, char* szPrice)
{
	if (!myConnect())
	{
		myClose();
		return NULL;
	}
	char szBuy[900] = { 0 };
	memset(nowTime, 0, 30);
	getNowTime(nowTime);
	//strRMBID.data()

	sprintf(szBuy, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"true\\\" Lot=\\\"%s\\\" IsBuy=\\\"true\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\"/>\\n</Transaction>\\n\"},\"Method\":\"Place\"}", GetUUID(uuid).data(), szInstrumentID, GetUUID(uuid).data(), nowTime, strAccountId, nowTime, UserId, szLot, szPrice, GetUUID(uuid).data(), szLot);
	//{27b3bacd-99bc-4765-a590-f9f97f39b974}{"Arguments":{"transactionXml":"<Transaction Type=\"0\" OrderType=\"0\" PlacingSettingType=\"0\" AccountID=\"ab9549b4-87b9-403c-b521-4a0da138c352\" InstrumentID=\"0c4188c7-376a-446d-b485-462f37c58536\" EndTime=\"1800-01-01 00:00:00\" ExpireTime=\"\" SubType=\"0\" SubmitTime=\"2021-03-02 08:54:02\" ExpireType=\"0\" SubmitorID=\"186a1d8e-3de8-4b46-b47e-0c109de82f06\"ID=\"b3e7a57a-4f9d-404e-976d-90e3e6492b37\" OrganizationCode=\"\" BeginTime=\"2021-03-02 08:54:02\">\n <Order SetPrice=\"359.62\" SetPrice2=\"\" DQMaxMove=\"0\" IsOpen=\"true\" ID=\"14ada727-fe2a-48f5-9318-3ac819b18d33\" IsBuy=\"true\" TradeOption=\"0\" Lot=\"1\" OriginalLot=\"1\" PhysicalTradeSide=\"0\"/>\n</Transaction>\n"},"Method":"Place"}
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szBuy) + 8 + nSessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + nSessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szBuy) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, SessionId, nSessionLen);
	memcpy(buffer + 8 + nSessionLen, szBuy, strlen(szBuy));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	char* pRead = readData(readbuffer);
	myClose();
	return pRead;
}



extern "C" __declspec(dllexport)
char* GetChartQuotaion(char* szInstrumentID)
{
	if (!myConnect())
	{
		myClose();
		return NULL;
	}
	char qd[555] = { 0 };
	char nowtime[30] = { 0 };
	memset(buffer, 0, MAXBUF);
	getNowTime2(nowtime);
	sprintf(qd, "{%s}{\"Arguments\":{\"accountID\":\"\",\"dataCycleParameter\":\"M1\",\"from\":\"%s\",\"instrumentID\":\"%s\",\"quotePolicyID\":\"4c1dba2d-6007-4df3-93cd-6cf61afd0332\",\"rangeType\":0,\"requestType\":1,\"to\":\"%s\"},\"Method\":\"AsyncGetChartData2ForCppTrader\"}", GetUUID(uuid).data(), nowtime, szInstrumentID, nowtime);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(qd) + 8 + nSessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + nSessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(qd) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, SessionId, nSessionLen);
	memcpy(buffer + 8 + nSessionLen, qd, strlen(qd));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	char* pRead = readData(readbuffer);
	myClose();
	return pRead;
}

extern "C" __declspec(dllexport)
void HeartPeer()
{
	if (!myConnect())
	{
		myClose();
		return;
	}
	char qd[255] = { 0 };
	memset(buffer, 0, MAXBUF);
	sprintf(qd, "{%s}", GetUUID(uuid).data());
	//{27b3bacd - 99bc - 4765 - a590 - f9f97f39b974}
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(qd) + 8 + nSessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1005;
	myHeader2.nNum2 = 0x2600 + nSessionLen;
	myHeader2.nNum3 = 0;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, SessionId, nSessionLen);
	memcpy(buffer + 8 + nSessionLen, qd, strlen(qd));
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	readData(readbuffer);
	myClose();
}

BOOL  APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:

	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

