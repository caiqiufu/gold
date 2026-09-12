//真实账号IP：75.2.85.181
//模拟账号IP：18.166.151.60
//pathName真实模拟都是WFB
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

#pragma comment(lib, "Ws2_32.lib")

#if defined(WIN32)||defined(WINCE)||defined(WIN64)
#include <objbase.h>
#else
#include <uuid/uuid.h> 
#endif



#define MAXBUF 1520240
using namespace std;
std::string uuid;

char buffer[MAXBUF] = { 0 };
static char readBuffer[MAXBUF] = { 0 };
static char tem[1520240] = { 0 };
char nowTime[30] = { 0 };
//用户ID
static char _UserId[100] = { 0 };
//Session Id
static char _SessionId[5] = { 0 };
//Session 长度
unsigned short int _SessionLen = 0;
//账户ID
static char _AccountId[100] = { 0 };
//合约ID
static char _InstrumentId[100] = { 0 };
//合约报价策略ID
static char _QuotePolicyId[100] = { 0 };
//Bid Price
static double _BidPrice = 0;
//Ask Price
static double _AskPrice = 0;

TiXmlDocument InitDataXML;
TiXmlDocument SettingDataXML;
TiXmlDocument TradingDataXML;

// 定义全局变量
//OpenSSL的决大部分API函数都在围绕这两种数据结构体完成TLS握手和数据加解密工作.
SSL_CTX* _CTX;
//SSL 通信
SSL* _SSL;
//socket监听套接字
int _SOCK_FD;
//结构体用来处理网络通信的地址
struct sockaddr_in _DEST;
//针对获取行情的参数 begin
int _SOCK_FD_QUOTE;
struct sockaddr_in _DEST_QUOTE;
static char _SessionId_QUOTE[5] = { 0 };
char _StrDeCode[500] = { 0 };
char _Watchwords[100] = { 0 };
char _ClientId[100] = { 0 };
unsigned short int _SessionLen_Quote = 0;
static bool _ConnectFlag = false;
static bool _ConnectQuoteFlag = false;
//用户和行情登录完成
static bool _LoginCompletedFlag = false;
//针对获取行情的参数 end

//UUID 是 通用唯一识别码（Universally Unique Identifier）的缩写，是一种软件建构的标准，亦为开放软件基金会组织在分布式计算环境领域的一部分
std::string _UUID;

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
char* GetUseridX()
{
	return _UserId;

}

extern "C" __declspec(dllexport)
char* GetSessionid()
{
	return _SessionId;

}

extern "C" __declspec(dllexport)
char* GetAccountid()
{
	return _AccountId;

}

extern "C" __declspec(dllexport)
char* GetClientid()
{
	//GetClientId
	return _ClientId;

}



extern "C" __declspec(dllexport)
int GetSessionlen()
{
	return _SessionLen;

}




extern "C" __declspec(dllexport)
void SetUserid(char * _UserId0)
{

	 strcpy(_UserId, _UserId0);
}

extern "C" __declspec(dllexport)
void SetSessionid(char* _SessionId0)
{
	strcpy(_SessionId, _SessionId0);

}

extern "C" __declspec(dllexport)
void SetAccountid(char * _AccountId0)
{
	strcpy(_AccountId,_AccountId0);

}


extern "C" __declspec(dllexport)
void SetSessionlen(int len)
{
	_SessionLen = len;

}

extern "C" __declspec(dllexport)
void SetClientid(char* _AccountId0)
{

	strcpy(_ClientId, _AccountId0);

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
}
char* WriteF(char* buf, int len)
{
	unsigned int newlen = 0;
	uLongf dlen = 920240;
	uLongf slen = len - 0x26 - 8 - _SessionLen;
	memset(tem, 0, 920240);
	int nRet = uncompress((Bytef*)tem, &dlen, (Bytef*)buf + 8 + _SessionLen + 0x26, slen);
	char strDesGbk[920240] = { 0 };
	utf8ToGbk(tem, strDesGbk);
	if (nRet == 0)return strDesGbk;
	return buf + 8 + _SessionLen + 0x26;
}

char* readData(char* buffer)
{
	int nRead = 0;
	int len = 0;
	memset(&myHeader, 0, sizeof(myHeader));
	do {
		len = SSL_read(_SSL, (char*)(&myHeader) + nRead, sizeof(myHeader) - nRead);
		nRead += len;
	} while (nRead < sizeof(myHeader));
	//myHeader.nNum1 = 100;
	nRead = 0;
	while (nRead < myHeader.nNum1)
	{
		len = SSL_read(_SSL, buffer + nRead, myHeader.nNum1 - nRead);
		nRead += len;
		if (len <= 0) {
			break;
		}
	}
	return WriteF(buffer, nRead);
}

int  writeData(char* buffer, int nLen)
{
	int len = SSL_write(_SSL, buffer, nLen);
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
/// <summary>
/// 获取SessionId
/// </summary>
/// <param name="buf"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetSessionId(char* buf)
{
	char* p = strstr(buf, "SessionId");
	if (p)
	{
		memset(_SessionId, 0, sizeof(_SessionId));
		p += 12;
		char* pEnd = strstr(p, "\"");
		memcpy(_SessionId, p, pEnd - p);
		_SessionLen = strlen(_SessionId);
		return _SessionId;
	}
	return NULL;
}
/// <summary>
/// 获取UserId
/// </summary>
/// <param name="buf"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetUserId(char* buf)
{
	char* p = strstr(buf, "UserId");
	if (p)
	{
		memset(_UserId, 0, sizeof(_UserId));
		p += 9;
		char* pEnd = strstr(p, "\"");
		memcpy(_UserId, p, pEnd - p);
		return _UserId;
	}
	return NULL;
}
/// <summary>
/// 获取ClientId
/// </summary>
/// <param name="buf"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetClientId(char* buf)
{
	char* p = strstr(buf, "ClientId");
	if (p)
	{
		memset(_ClientId, 0, sizeof(_ClientId));
		p += 11;
		char* pEnd = strstr(p, "\"");
		memcpy(_ClientId, p, pEnd - p);
		return _ClientId;
	}
	return NULL;
}
/// <summary>
/// 创建连接
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
bool Connect()
{
	// OpenSSL初始化
	SSL_library_init();
	// 添加SSL的加密/HASH算法
	OpenSSL_add_all_algorithms();
	// 算法初始化  
	// 加载SSL错误信息
	SSL_load_error_strings();
	// 申请SSL会话环境
	//SSL_CTX_new创建ssl上下文，这里面很多全局变量要被各个阶段共享。
	_CTX = SSL_CTX_new(TLSv1_client_method());//SSLv23_client_method
	if ((_SOCK_FD = socket(AF_INET, SOCK_STREAM, 0)) < 0) {
		return false;
	}
	struct sockaddr_in dst = _DEST;
	if (connect(_SOCK_FD, (struct sockaddr*)&_DEST, sizeof(_DEST)) != 0) {
		return false;
	}
	_SSL = SSL_new(_CTX);
	SSL_set_fd(_SSL, _SOCK_FD);
	if (SSL_connect(_SSL) == -1) {
		unsigned long ulErr = ERR_get_error(); // 获取错误号
		char szErrMsg[1024] = { 0 };
		char szErrGBK[1024] = { 0 };
		char* pTmp = NULL;
		pTmp = ERR_error_string(ulErr, szErrMsg);
		return false;
	}
	_ConnectFlag = true;
	return true;
}
/// <summary>
/// 获取行情信息连接
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
bool ConnectQuote()
{
	if ((_SOCK_FD_QUOTE = socket(AF_INET, SOCK_STREAM, 0)) < 0) {
		return false;
	}
	if (connect(_SOCK_FD_QUOTE, (struct sockaddr*)&_DEST_QUOTE, sizeof(_DEST_QUOTE)) != 0) {
		return false;
	}
	_ConnectQuoteFlag = true;
	return true;
}
extern "C" __declspec(dllexport)
bool isConnect()
{
	_LoginCompletedFlag = false;
	return _ConnectFlag;
}
extern "C" __declspec(dllexport)
bool isConnectQuote()
{
	_LoginCompletedFlag = false;
	return _ConnectQuoteFlag;
}

extern "C" __declspec(dllexport)
bool isLoginCompleted()
{
	return _LoginCompletedFlag;
}
/// <summary>
/// 关闭连接
/// </summary>
extern "C" __declspec(dllexport)
bool DisConnect()
{
	if (_ConnectFlag)
	{
		SSL_shutdown(_SSL);
		SSL_free(_SSL);
		closesocket(_SOCK_FD);
		SSL_CTX_free(_CTX);
		// 蔡秋伏，新修改的代码
		//SSL_CTX_free(_CTX);
		//SSL_shutdown(_SSL);
		//SSL_free(_SSL);
		//ERR_free_strings();
	}
	_ConnectFlag = false;
	return true;
}
/// <summary>
/// 断开行情连接
/// </summary>
extern "C" __declspec(dllexport)
bool DisConnectQuote()
{
	closesocket(_SOCK_FD_QUOTE);
	_ConnectQuoteFlag = false;
	return true;
}
/// <summary>
/// 获取设置参数
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetSettingData()
{
	char szGetSettingData[255] = { 0 };
	sprintf(szGetSettingData, "{%s}{\"Arguments\":{}, \"Method\":\"GetSettingData\"}\0", GetUUID(uuid).data());
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szGetSettingData) + 8 + _SessionLen;
	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szGetSettingData) - 0x26;
	myHeader2.nNum4 = 0x0000;
	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, szGetSettingData, strlen(szGetSettingData));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	DisConnect();
	return pRead;
}
/// <summary>
/// 获取交易参数，账户信息、订单信息
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetTradingData()
{
	if (!isConnect())
	{
		Connect();
	}
	char szGetTradingData[500] = { 0 };
	sprintf(szGetTradingData, "{%s}{\"Arguments\":{\"ids\":[\"%s\"] }, \"Method\" : \"GetTradingData\"}", GetUUID(uuid).data(), _AccountId);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szGetTradingData) + 8 + _SessionLen;
	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szGetTradingData) - 0x26;
	myHeader2.nNum4 = 0x0000;
	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, szGetTradingData, strlen(szGetTradingData));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	DisConnect();
	return pRead;
}
/// <summary>
/// 获取初始化数据
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetInitData()
{
	char szGetInitData[255] = { 0 };
	sprintf(szGetInitData, "{%s}{\"Arguments\":{}, \"Method\":\"GetInitData\"}\0", GetUUID(uuid).data());
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szGetInitData) + 8 + _SessionLen;
	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szGetInitData) - 0x26;
	myHeader2.nNum4 = 0x0000;
	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, szGetInitData, strlen(szGetInitData));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	return pRead;
}
/// <summary>
/// 设置初始化数据
/// </summary>
/// <param name="accountId"></param>
/// <param name="instrumentId"></param>
/// <param name="quotePolicyId"></param>
extern "C" __declspec(dllexport)
void SetInitParas(char* accountId, char* instrumentId, char* quotePolicyId)
{
	memset(_AccountId, 0, 100);
	strcpy(_AccountId, accountId);
	memset(_InstrumentId, 0, 100);
	strcpy(_InstrumentId, instrumentId);
	memset(_QuotePolicyId, 0, 100);
	strcpy(_QuotePolicyId, quotePolicyId);
}
/// <summary>
/// 获取初始化数据
/// </summary>
/// <param name="szName"></param>
extern "C" __declspec(dllexport)
char* GetInitParas()
{
	char paras[1000] = { 0 };
	strcat(paras, _SessionId);
	strcat(paras, "|");
	strcat(paras, _UserId);
	strcat(paras, "|");
	strcat(paras, _AccountId);
	strcat(paras, "|");
	strcat(paras, _InstrumentId);
	strcat(paras, "|");
	strcat(paras, _QuotePolicyId);
	strcat(paras, "|");
	strcat(paras, _ClientId);
	strcat(paras, "|");
	strcat(paras, _SessionId_QUOTE);
	strcat(paras, "|");
	strcat(paras, _Watchwords);
	strcat(paras, "|");
	return paras;
}
/// <summary>
/// 读取通信数据
/// </summary>
/// <param name="dataBuffer">读取数据缓冲区</param>
/// <returns></returns>
int myReadData(char* dataBuffer)
{
	int nRead = 0;
	int len = 0;
	struct Header messageHeader;
	memset(&messageHeader, 0, sizeof(messageHeader));
	//读取消息头，获取消息长度
	SSL_read(_SSL, &messageHeader, sizeof(messageHeader));
	while (nRead < messageHeader.nNum1)
	{
		len = SSL_read(_SSL, dataBuffer + nRead, messageHeader.nNum1 - nRead);
		nRead += len;
		if (len <= 0) {
			break;
		}
	}
	return nRead;
}
/// <summary>
/// 登录
/// </summary>
/// <param name="userName"></param>
/// <param name="password"></param>
/// <param name="accountType"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* Login(char* userName, char* password, char* accountType)
{
	if (!isConnect())
	{
		Connect();
	}
	char szLogin2[1200] = { 0 };
	sprintf(szLogin2, "{%s}{\"Arguments\":{\"appType\":\"18\",\"containsMilSInQuotationTime\":false,\"descriptionInfo\":\"Version:1.0.0.3 System:Operating system version 10.0, corresponds to Windows 10, introduced in Qt 5.5 CPU:x86_64\",\"isGuestIdentity\":false,\"language\":\"CHS\",\"loginID\":\"%s\",\"password\":\"%s\",\"sendQuotationInSsl\":false,\"validPathName\":\"%s\"},\"Method\":\"Login\"}\0", GetUUID(uuid).data(), userName, password, accountType);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szLogin2) + 8;
	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600;
	myHeader2.nNum3 = strlen(szLogin2) - 0x26;
	myHeader2.nNum4 = 0x0000;
	memset(buffer, 0, MAXBUF);
	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, szLogin2, strlen(szLogin2));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pReadBuff = readData(readBuffer);
	if (pReadBuff)
	{
		_SessionLen = strlen(_SessionId);
		GetSessionId(pReadBuff);
		GetUserId(pReadBuff);
		GetClientId(pReadBuff);
	}
	return pReadBuff;
}
/// <summary>
/// 行情解码
/// </summary>
/// <param name="src"></param>
/// <param name="len"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* DeCode(char* src, int len)
{
	char dictionary[100] = { 0x00,0x2D,0x2E,0x2F,0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x3A,0x3B,0x3A,0x2F,0x3B,0x2D };
	memset(_StrDeCode, 0, 500);
	for (int i = 0, j = 0; i < len; i++, j++)
	{
		unsigned char nIndex = src[i] >> 4;
		nIndex = nIndex & 0x0f;
		_StrDeCode[j] = dictionary[(unsigned int)nIndex];
		unsigned char nIndex2 = src[i] & 0x0f;
		if (nIndex2 == 0)
		{
			continue;
		}
		j++;
		_StrDeCode[j] = dictionary[(int)nIndex2];
	}
	return _StrDeCode;
}

char* RecvAll2(char* buffer, int size)
{
	int nRead = 0;
	int len = 0;
	memset(&myHeader, 0, sizeof(myHeader));
	recv(_SOCK_FD_QUOTE, (char*)&myHeader, sizeof(myHeader), 0);
	while (nRead < myHeader.nNum1)
	{
		len = recv(_SOCK_FD_QUOTE, buffer + nRead, myHeader.nNum1 - nRead, 0); //SSL_read(ssl, buffer + nRead, myHeader.nNum1 - nRead);
		nRead += len;
		if (len <= 0) {
			break;
		}
	}
	return DeCode(buffer + 8, nRead - 8);
}

char* RecvAll(char* buffer, int size)
{
	int nRead = 0;
	int len = 0;
	memset(&myHeader, 0, sizeof(myHeader));
	recv(_SOCK_FD_QUOTE, (char*)&myHeader, sizeof(myHeader), 0);
	while (nRead < myHeader.nNum1)
	{
		len = recv(_SOCK_FD_QUOTE, buffer + nRead, myHeader.nNum1 - nRead, 0);
		nRead += len;
		if (len <= 0) {
			break;
		}
	}
	return WriteF(buffer, nRead);
}
/// <summary>
/// 行情心跳
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* HeartPeerQuote()
{
	char qd[255] = { 0 };
	memset(buffer, 0, MAXBUF);
	sprintf(qd, "{%s}", GetUUID(uuid).data());
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(qd) + 8 + _SessionLen_Quote;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1005;
	myHeader2.nNum2 = 0x2600 + _SessionLen_Quote;
	myHeader2.nNum3 = 0;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId_QUOTE, _SessionLen_Quote);
	memcpy(buffer + 8 + _SessionLen_Quote, qd, strlen(qd));
	int nRet = send(_SOCK_FD_QUOTE, (char*)&myHeader, sizeof(myHeader), 0);
	nRet = send(_SOCK_FD_QUOTE, buffer, myHeader.nNum1, 0);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = RecvAll(readBuffer, MAXBUF);
	//pRead = RecvAll(sockfd2, readbuffer, MAXBUF);
	//DisConnectQuote();
	return pRead;
}

/// <summary>
/// 接收行情数据并解密
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* RecvData2()
{
	return RecvAll2(readBuffer, 0);
}
char bid[10] = { 0 };
char ask[10] = { 0 };
/// <summary>
/// 获取Bid Price
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
double GetBidPrice()
{
	char* quoteStr = RecvAll2(readBuffer, 0);
	memset(bid, 0, sizeof(bid));
	memcpy(bid, quoteStr + 3, 7);
	return atof(bid);
}
/// <summary>
/// 获取Ask Price
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
double GetAskPrice()
{
	char* quoteStr = RecvAll2(readBuffer, 0);
	memset(ask, 0, sizeof(ask));
	memcpy(ask, quoteStr + 11, 7);
	return  atof(ask);
}
/// <summary>
/// 心跳
/// </summary>
extern "C" __declspec(dllexport)
char* HeartPeer()
{
	if (!isConnect())
	{
		Connect();
	}
	char qd[255] = { 0 };
	memset(buffer, 0, MAXBUF);
	sprintf(qd, "{%s}", GetUUID(uuid).data());
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(qd) + 8 + _SessionLen;
	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1005;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = 0;
	myHeader2.nNum4 = 0x0000;
	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, qd, strlen(qd));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	DisConnect();
	return pRead;
}
/// <summary>
/// 获取订单的实时报价
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetQuote()
{
	if (!isConnectQuote())
	{
		ConnectQuote();
	}
	memset(readBuffer, 0, MAXBUF);
	char* quoteStr = RecvAll2(readBuffer, 0);
	//char quoteStr[200] = "19:1726.10:1725.60:1742.60:1724.70::314934274:::36:1743.10:1725.20;";
	memset(bid, 0, sizeof(bid));
	memset(ask, 0, sizeof(ask));
	memcpy(bid, quoteStr + 3, 7);
	memcpy(ask, quoteStr + 11, 7);
	char price[20] = { 0 };
	memset(price, 0, sizeof(price));
	memcpy(price, &bid, 7);
	memcpy(price, &ask, 7);
	DisConnectQuote();
	return quoteStr;
}
/// <summary>
/// 初始化连接的Ip和端口
/// </summary>
/// <param name="ip"></param>
/// <param name="port"></param>
extern "C" __declspec(dllexport)
void  init(char* ip, int port)
{
	WORD ver = MAKEWORD(2, 2);
	WSADATA dat;
	WSAStartup(ver, &dat);
	memset(&_DEST, 0, sizeof(_DEST));
	_DEST.sin_family = AF_INET;
	_DEST.sin_port = htons(port);//4523
	_DEST.sin_addr.s_addr = inet_addr(ip);//"203.160.75.182"
}
/// <summary>
/// 获取显示的报价
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetDisplayQuote()
{
	if (!isConnect())
	{
		Connect();
	}
	char qd[555] = { 0 };
	char nowtime[30] = { 0 };
	memset(buffer, 0, MAXBUF);
	getNowTime2(nowtime);
	sprintf(qd, "{%s}{\"Arguments\":{\"accountID\":\"\",\"dataCycleParameter\":\"M1\",\"from\":\"%s\",\"instrumentID\":\"%s\",\"quotePolicyID\":\"4c1dba2d-6007-4df3-93cd-6cf61afd0332\",\"rangeType\":0,\"requestType\":1,\"to\":\"%s\"},\"Method\":\"AsyncGetChartData2ForCppTrader\"}", GetUUID(uuid).data(), nowtime, _InstrumentId, nowtime);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(qd) + 8 + _SessionLen;
	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(qd) - 0x26;
	myHeader2.nNum4 = 0x0000;
	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, qd, strlen(qd));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	DisConnect();
	return pRead;
}
/// <summary>
/// 拼装返回信息
/// </summary>
/// <param name="price"></param>
/// <param name="pRead"></param>
/// <returns></returns>
char* GetOrderReturn(char* price, char* pRead)
{
	char paras[1000] = { 0 };
	strcat(paras, price);
	strcat(paras, "|");
	strcat(paras, pRead);
	return paras;
}

/// <summary>
/// 开仓买多
/// </summary>
/// <param name="price"></param>
/// <param name="slippage"></param>
/// <param name="lot"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* OpenBuyOrder(double price, double slippage, double lot)
{
	try {
		if (!isConnect())
		{
			Connect();
		}
		/*
		double priced = atof(price);
		double slippaged = atof(slippage);
		if (priced == 0)
		{
			double bidPrice = GetBidPrice();
			priced = bidPrice + slippaged;
		}
		else
		{
			priced = priced + slippaged;
		}
		char pp2[20];
		sprintf(pp2, "%.2f", priced);
		*/
		if (price == 0)
		{
			double bidPrice = GetBidPrice();
			price = bidPrice + slippage;
		}
		else
		{
			price = price + slippage;
		}
		char pp2[20];
		sprintf(pp2, "%.2f", price);

		char lot22[20];
		sprintf(lot22, "%.2f", lot);

		char szBuy[900] = { 0 };
		memset(nowTime, 0, 30);
		getNowTime(nowTime);
		sprintf(szBuy, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"true\\\" Lot=\\\"%s\\\" IsBuy=\\\"true\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\"/>\\n</Transaction>\\n\"},\"Method\":\"Place\"}", GetUUID(uuid).data(), _InstrumentId, GetUUID(uuid).data(), nowTime, _AccountId, nowTime, _UserId, lot22, pp2, GetUUID(uuid).data(), lot22);
		//{27b3bacd-99bc-4765-a590-f9f97f39b974}{"Arguments":{"transactionXml":"<Transaction Type=\"0\" OrderType=\"0\" PlacingSettingType=\"0\" AccountID=\"ab9549b4-87b9-403c-b521-4a0da138c352\" InstrumentID=\"0c4188c7-376a-446d-b485-462f37c58536\" EndTime=\"1800-01-01 00:00:00\" ExpireTime=\"\" SubType=\"0\" SubmitTime=\"2021-03-02 08:54:02\" ExpireType=\"0\" SubmitorID=\"186a1d8e-3de8-4b46-b47e-0c109de82f06\"ID=\"b3e7a57a-4f9d-404e-976d-90e3e6492b37\" OrganizationCode=\"\" BeginTime=\"2021-03-02 08:54:02\">\n <Order SetPrice=\"359.62\" SetPrice2=\"\" DQMaxMove=\"0\" IsOpen=\"true\" ID=\"14ada727-fe2a-48f5-9318-3ac819b18d33\" IsBuy=\"true\" TradeOption=\"0\" Lot=\"1\" OriginalLot=\"1\" PhysicalTradeSide=\"0\"/>\n</Transaction>\n"},"Method":"Place"};
		memset(&myHeader, 0, sizeof(myHeader));
		myHeader.A = 0x80;
		myHeader.nNum1 = strlen(szBuy) + 8 + _SessionLen;
		memset(&myHeader2, 0, sizeof(myHeader2));
		myHeader2.nNum1 = 0x1001;
		myHeader2.nNum2 = 0x2600 + _SessionLen;
		myHeader2.nNum3 = (unsigned short int)strlen(szBuy) - 0x26;
		myHeader2.nNum4 = 0x0000;
		memcpy(buffer, &myHeader2, 8);
		memcpy(buffer + 8, _SessionId, _SessionLen);
		memcpy(buffer + 8 + _SessionLen, szBuy, strlen(szBuy));
		SSL_write(_SSL, &myHeader, sizeof(myHeader));
		SSL_write(_SSL, buffer, myHeader.nNum1);
		memset(readBuffer, 0, MAXBUF);
		char* pRead = readData(readBuffer);
		DisConnect();
		//return pRead;
		return GetOrderReturn(pp2, pRead);
	}
	catch (std::exception& ex) {
		char* buf = new char[strlen(ex.what()) + 1];
		strcpy(buf, ex.what());
		return buf;
	}

}
/// <summary>
/// 开仓卖空
/// </summary>
/// <param name="price"></param>
/// <param name="slippage"></param>
/// <param name="lot"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* OpenSellOrder(double price, double slippage, double lot)
{
	try
	{
		if (!isConnect())
		{
			Connect();
		}
		/*
		double priced = atof(price);
		double slippaged = atof(slippage);
		if (priced == 0)
		{
			double askPrice = GetAskPrice();
			priced = askPrice + slippaged;
		}
		else
		{
			priced = priced + slippaged;
		}
		*/
		if (price == 0)
		{
			double askPrice = GetAskPrice();
			price = askPrice - slippage;
		}
		else
		{
			price = price - slippage;
		}


		char pp2[20];
		sprintf(pp2, "%.2f", price);

		char lot22[20];
		sprintf(lot22, "%.2f", lot);

		memset(nowTime, 0, 30);
		getNowTime(nowTime);
		char szSell[900] = { 0 };
		sprintf(szSell, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"true\\\" Lot=\\\"%s\\\" IsBuy=\\\"false\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\"/>\\n</Transaction>\\n\"}, \"Method\":\"Place\"}", GetUUID(uuid).data(), _InstrumentId, GetUUID(uuid).data(), nowTime, _AccountId, nowTime, _UserId, lot22, pp2, GetUUID(uuid).data(), lot22);
		memset(&myHeader, 0, sizeof(myHeader));
		myHeader.A = 0x80;
		myHeader.nNum1 = strlen(szSell) + 8 + _SessionLen;
		memset(&myHeader2, 0, sizeof(myHeader2));
		myHeader2.nNum1 = 0x1001;
		myHeader2.nNum2 = 0x2600 + _SessionLen;
		myHeader2.nNum3 = (unsigned short int)strlen(szSell) - 0x26;
		myHeader2.nNum4 = 0x0000;
		memcpy(buffer, &myHeader2, 8);
		memcpy(buffer + 8, _SessionId, _SessionLen);
		memcpy(buffer + 8 + _SessionLen, szSell, strlen(szSell));
		SSL_write(_SSL, &myHeader, sizeof(myHeader));
		SSL_write(_SSL, buffer, myHeader.nNum1);
		memset(readBuffer, 0, MAXBUF);
		char* pRead = readData(readBuffer);
		DisConnect();
		//return pRead;
		return GetOrderReturn(pp2, pRead);
	}
	catch (std::exception& ex) {
		char* buf = new char[strlen(ex.what()) + 1];
		strcpy(buf, ex.what());
		return buf;
	}
}
/// <summary>
/// 平仓
/// </summary>
/// <param name="nowPrice">平仓价格</param>
/// <param name="slippage">偏差</param>
/// <param name="openPrice">Order/SetPrice</param>
/// <param name="lot">Order/Lot</param>
/// <param name="orderID">Order/ID</param>
/// <param name="openTime">Order/InterestValueDate</param>
/// <param name="isBuy">Order/IsBuy  如果订单是IsBuy=true，则值为false，反之亦然</param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* CloseOrder(double nowPrice, double slippage, double openPrice, double lot, char* orderID, char* openTime, char* isBuy)
{
	if (!isConnect())
	{
		Connect();
	}
	/*
	double nowPriced = atof(nowPrice);
	double slippaged = atof(slippage);
	if (nowPriced == 0)
	{
		if (strcmp(isBuy, "true") == 0)
		{
			double askPrice = GetAskPrice();
			nowPriced = askPrice - slippaged;
		}
		else
		{
			double bidPrice = GetBidPrice();
			nowPriced = bidPrice + slippaged;
		}
	}
	else
	{
		if (strcmp(isBuy, "true") == 0)
		{
			nowPriced = nowPriced - slippaged;
		}
		else
		{
			nowPriced = nowPriced + slippaged;
		}
	}
	*/

	if (nowPrice == 0)
	{
		if (strcmp(isBuy, "true") == 0)
		{
			double askPrice = GetAskPrice();
			nowPrice = askPrice - slippage;
		}
		else
		{
			double bidPrice = GetBidPrice();
			nowPrice = bidPrice + slippage;
		}
	}
	else
	{
		if (strcmp(isBuy, "true") == 0)
		{
			nowPrice = nowPrice - slippage;
		}
		else
		{
			nowPrice = nowPrice + slippage;
		}
	}

	char isBuy22[10] = { 0 };
	if (strcmp(isBuy, "true") == 0)
	{
		strcpy(isBuy22, "false");
	}
	else
	{
		strcpy(isBuy22, "true");
	}
	char pp[20];
	sprintf(pp, "%.2f", nowPrice);

	char pp2[20];
	sprintf(pp2, "%.2f", openPrice);

	char lot22[20];
	sprintf(lot22, "%.2f", lot);

	memset(nowTime, 0, 30);
	getNowTime(nowTime);
	char szCloseAll[1000] = { 0 };
	sprintf(szCloseAll, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"false\\\" Lot=\\\"%s\\\" IsBuy=\\\"%s\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\">\n  <OrderRelation OpenOrderTime=\\\"%s\\\" OpenOrderID=\\\"%s\\\" OpenOrderPrice=\\\"%s\\\" ClosedLot=\\\"%s\\\"/>\n </Order>\\n</Transaction>\\n\"}, \"Method\":\"Place\"}",
		GetUUID(uuid).data(), _InstrumentId, GetUUID(uuid).data(), nowTime, _AccountId, nowTime, _UserId, lot22, pp, GetUUID(uuid).data(), lot22, isBuy22, openTime, orderID, pp2, lot22);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szCloseAll) + 8 + _SessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(szCloseAll) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, szCloseAll, strlen(szCloseAll));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	DisConnect();
	//return pRead;
	return GetOrderReturn(pp, pRead);
}


/// <summary>
/// 初始化获取行情的Ip和端口
/// </summary>
/// <param name="szIP"></param>
/// <param name="nPort"></param>
extern "C" __declspec(dllexport)
void initQuote(char* szIP, int nPort)
{
	memset(&_DEST_QUOTE, 0, sizeof(_DEST_QUOTE));
	_DEST_QUOTE.sin_family = AF_INET;
	_DEST_QUOTE.sin_port = htons(nPort);//4529
	_DEST_QUOTE.sin_addr.s_addr = inet_addr(szIP);//"203.160.75.182"
}
/// <summary>
/// 获取行情信息的SessionId
/// </summary>
/// <param name="buf"></param>
/// <returns></returns>
char* GetSessionIdQuote(char* buf)
{
	char* p = strstr(buf, "SessionId");
	if (p)
	{
		memset(_SessionId_QUOTE, 0, sizeof(_SessionId_QUOTE));
		p += 12;
		char* pEnd = strstr(p, "\"");
		memcpy(_SessionId_QUOTE, p, pEnd - p);
		return _SessionId_QUOTE;
	}
	return NULL;
}

/// <summary>
/// 获取行情worlds
/// </summary>
/// <param name="clientId"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetWatchWorlds(char* clientId)
{
	if (!isConnect())
	{
		Connect();
	}
	char qd[555] = { 0 };
	sprintf(qd, "{%s}{\"Arguments\":{\"clientId\":\"%s\",\"count\":5},\"Method\":\"GetWatchWorlds\"}", GetUUID(uuid).data(), clientId);
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(qd) + 8 + _SessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = (unsigned short int)strlen(qd) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, qd, strlen(qd));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	DisConnect();
	return pRead;
}
/// <summary>
/// 获取行情信息的world
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetWatchword()
{
	char* worlds = GetWatchWorlds(_ClientId);
	char* p = strstr(worlds + 20, "Watchwords\":[\"");
	if (p)
	{
		memset(_Watchwords, 0, sizeof(_Watchwords));
		p += 14;
		char* pEnd = strstr(p, "\"");
		memcpy(_Watchwords, p, pEnd - p);
		return _Watchwords;
	}
	return NULL;
}
/// <summary>
/// 获取服务器时间
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetServerDateTime()
{
	if (!isConnect())
	{
		Connect();
	}
	char qd[255] = { 0 };
	memset(buffer, 0, MAXBUF);
	sprintf(qd, "{%s}{\"Arguments\":{},\"Method\":\"GetServerDateTime\"}", GetUUID(uuid).data());
	//{27b3bacd - 99bc - 4765 - a590 - f9f97f39b974}
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(qd) + 8 + _SessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1005;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = strlen(qd) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, qd, strlen(qd));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	DisConnect();
	return pRead;
}
extern "C" __declspec(dllexport)
char* Recover()
{
	if (!isConnect())
	{
		Connect();
	}
	char qd[255] = { 0 };
	memset(buffer, 0, MAXBUF);
	sprintf(qd, "{%s}{\"Arguments\":{},\"Method\":\"Recover\"}", GetUUID(uuid).data());
	//{27b3bacd - 99bc - 4765 - a590 - f9f97f39b974}
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(qd) + 8 + _SessionLen;

	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1005;
	myHeader2.nNum2 = 0x2600 + _SessionLen;
	myHeader2.nNum3 = strlen(qd) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, _SessionId, _SessionLen);
	memcpy(buffer + 8 + _SessionLen, qd, strlen(qd));
	SSL_write(_SSL, &myHeader, sizeof(myHeader));
	SSL_write(_SSL, buffer, myHeader.nNum1);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = readData(readBuffer);
	DisConnect();
	return pRead;
}

/// <summary>
/// 登录获取行情信息
/// </summary>
/// <param name="instrumentAndQuotePolicyIDs"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* LoginQuote()
{
	if (!isConnectQuote())
	{
		ConnectQuote();
	}
	char* resWorld = GetWatchword();
	char instrumentAndQuotePolicyIDs[500] = { 0 };
	strcat(instrumentAndQuotePolicyIDs, _InstrumentId);
	strcat(instrumentAndQuotePolicyIDs, "|");
	strcat(instrumentAndQuotePolicyIDs, _QuotePolicyId);
	char szLogin2[1200] = { 0 };
	sprintf(szLogin2, "{%s}{\"Arguments\":{\"clientId\":\"%s\",\"instrumentAndQuotePolicyIDs\":[\"%s,\"],\"isGuestIdentity\":false,\"userID\":\"%s\",\"watchword\":\"%s\"},\"Method\":\"Login\"}\0", GetUUID(uuid).data(), _ClientId, instrumentAndQuotePolicyIDs, _UserId, resWorld);
	//sprintf(szLogin2, "{%s}{\"Arguments\":{\"clientId\":\"%s\",\"instrumentAndQuotePolicyIDs\":[\"0c4188c7-376a-446d-b485-462f37c58536|4c1dba2d-6007-4df3-93cd-6cf61afd0332,\",\"55f6a2b3-51a8-466a-99d2-45897bbf13fb|4c1dba2d-6007-4df3-93cd-6cf61afd0332,\",\"822636d4-98d0-4edc-be16-1a665f6c85d9|4c1dba2d-6007-4df3-93cd-6cf61afd0332,\"],\"isGuestIdentity\":false,\"userID\":\"%s\",\"watchword\":\"%s\"},\"Method\":\"Login\"}\0", GetUUID(uuid).data(), _ClientId, _UserId, _Watchwords);//a14f127f-ad65-41ec-bee1-b1eba7fe7fcf
	memset(&myHeader, 0, sizeof(myHeader));
	myHeader.A = 0x80;
	myHeader.nNum1 = strlen(szLogin2) + 8;
	memset(&myHeader2, 0, sizeof(myHeader2));
	myHeader2.nNum1 = 0x1001;
	myHeader2.nNum2 = 0x2600;
	myHeader2.nNum3 = strlen(szLogin2) - 0x26;
	myHeader2.nNum4 = 0x0000;

	memset(buffer, 0, MAXBUF);
	memcpy(buffer, &myHeader2, 8);
	memcpy(buffer + 8, szLogin2, strlen(szLogin2));

	int nRet = send(_SOCK_FD_QUOTE, (char*)&myHeader, sizeof(myHeader), 0);
	nRet = send(_SOCK_FD_QUOTE, buffer, myHeader.nNum1, 0);
	memset(readBuffer, 0, MAXBUF);
	char* pRead = RecvAll(readBuffer, MAXBUF);
	GetSessionIdQuote(pRead);
	_LoginCompletedFlag = true;
	return pRead;
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

