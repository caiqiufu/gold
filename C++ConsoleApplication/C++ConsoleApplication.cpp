// ConsoleApplication1.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//
#define _WINSOCK_DEPRECATED_NO_WARNINGS 1
#define _CRT_SECURE_NO_WARNINGS 1
#include <iostream>

#include "Winsock2.h"
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <stdlib.h>
#include "openssl/rsa.h" 
#include "openssl/x509.h"
#include "openssl/pem.h"
#include "openssl/ssl.h"
#include "openssl/err.h"
#include "zlib.h"
#include "tinyxml.h"
#include <string.h>

#pragma comment(lib, "Ws2_32.lib")

#if defined(WIN32)||defined(WINCE)||defined(WIN64)
#include <objbase.h>
#else
#include <uuid/uuid.h> 
#endif

using namespace std;
// 定义全局变量
//OpenSSL的决大部分API函数都在围绕这两种数据结构体完成TLS握手和数据加解密工作.
SSL_CTX* _CTX;
//SSL 通信
SSL* _SSL;
//socket监听套接字
int _SOCK_FD;
//结构体用来处理网络通信的地址
struct sockaddr_in _DEST;
//UUID 是 通用唯一识别码（Universally Unique Identifier）的缩写，是一种软件建构的标准，亦为开放软件基金会组织在分布式计算环境领域的一部分
std::string _UUID;
#define MAXBUF 1520240
//临时数据缓存区
char tem[920240] = { 0 };

TiXmlDocument InitDataXML;
//订单交易所需信息
std::string strRMBID;
std::string strRMBpriceSell;
std::string strRMBpriceBuy;
std::string strDes;
std::string strAccountId;

/// <summary>
/// 公共参数定义
/// </summary>

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
static char _BidPrice[100] = { 0 };
//Ask Price
static char _AskPrice[100] = { 0 };


//数据包消息头 Beign
/*
unsigned char A;//1个字节
bool bflag;//1个字节
unsigned short int nNum1;//2个字节  消息的长度
unsigned short int nNum2;//2个字节
*/
struct Header
{
	unsigned char A;
	bool bflag;
	unsigned short int nNum1;
	unsigned short int nNum2;
}myHeader;

/*
nsigned short int nNum1;
unsigned short int nNum2;
unsigned short int nNum3; 消息长度
unsigned short int nNum4;
*/
struct Header2
{

	unsigned short int nNum1;
	unsigned short int nNum2;
	unsigned short int nNum3;
	unsigned short int nNum4;
}myHeader2;
//数据包消息头 End

//获取当前时间
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
/// <summary>
/// 创建连接
/// </summary>
/// <param name="ip">IP</param>
/// <param name="port">Port</param>
/// <returns></returns>
extern "C" _declspec(dllexport)
bool Connect(char* ip, char* port)
{
	memset(&_DEST, 0, sizeof(_DEST));
	_DEST.sin_family = AF_INET;
	//登录端口
	_DEST.sin_port = htons(atof(port));
	//登录IP
	_DEST.sin_addr.s_addr = inet_addr(ip);
	//创建一个被指定变量连接而成的WORD变量。返回一个WORD变量。
	WORD ver = MAKEWORD(2, 2);
	//被WSAStartup函数调用后返回的 Windows Sockets数据
	WSADATA dat;
	//wsastartup主要就是进行相应的socket库绑定
	WSAStartup(ver, &dat);
	//OpenSSL初始化
	SSL_library_init();
	//密钥有经过口令加密需要这个函数
	OpenSSL_add_all_algorithms();
	//错误信息初始化
	SSL_load_error_strings();
	//申请SSL会话环境
	_CTX = SSL_CTX_new(TLSv1_client_method());//SSLv23_client_method
	//生成一个TCP的socket,Function: int socket (int namespace, int style, int protocol)
	if ((_SOCK_FD = socket(AF_INET, SOCK_STREAM, 0)) < 0) {
		//	perror("Socket");
		return false;
	}
	/*
	connect函数的功能是完成一个有连接协议的连接过程，对于TCP来说就是那个三路握手过程
	int connect(
		SOCKET s,     // 没绑定套接字
		const struct sockaddr FAR *name,   // 目标地址指针，目标地址中必须包含IP和端口信息。
		int namelen   // name的长度
	);
	*/
	//主动连接服务器
	if (connect(_SOCK_FD, (struct sockaddr*)&_DEST, sizeof(_DEST)) != 0) {
		//	perror("Connect ");
		return false;
	}
	//printf("connectd server successly\n");
	/*基于 ctx 产生一个新的 SSL*/
	_SSL = SSL_new(_CTX);
	/*将连接用户的 socket 加入到 SSL*/
	SSL_set_fd(_SSL, _SOCK_FD);
	/*建立 SSL 连接*/
	if (SSL_connect(_SSL) == -1) {
		unsigned long ulErr = ERR_get_error(); // 获取错误号
		char szErrMsg[1024] = { 0 };
		char* pTmp = NULL;
		pTmp = ERR_error_string(ulErr, szErrMsg);
		printf(szErrMsg);
		printf(pTmp);
		return false;
	}
	return true;
}

/// <summary>
/// 关闭连接
/// </summary>
extern "C" _declspec(dllexport)
void DisConnection()
{
	SSL_shutdown(_SSL);
	SSL_free(_SSL);
	closesocket(_SOCK_FD);
	SSL_CTX_free(_CTX);
	printf("disconnect\n");
}
/// <summary>
/// 获取UUID
/// </summary>
/// <param name="strUUID"></param>
/// <returns></returns>
std::string GetUUID(std::string& strUUID)
{
	//GUID（Globally Unique Identifier）：全球唯一标识符，是一种由算法生成的字母数字标识符，长度为128位，在Windows平台上，
	//GUID被广泛应用于注册表、数据库、接口标识以及自动生成的目录名称、机器名称等。
	strUUID = "";
#if defined(WIN32)||defined(WINCE)||defined(WIN64)
	GUID guid;
	//Windows下提供了函数CoCreateGuid用于生成GUID
	if (!CoCreateGuid(&guid))
	{
		char buffer[64] = { 0 };
		//输出的字符串就算超过缓冲区长度，仍然会有输出，输出字符串被截断到count大小
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

/// <summary>
/// 编码转换
/// </summary>
/// <param name="utf8String"></param>
/// <param name="gbkString"></param>
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
/// <summary>
/// GBK打印输出
/// </summary>
/// <param name="utf"></param>
void gbkPrint(const char* utf)
{
	char strDesGbk[22500] = { 0 };
	utf8ToGbk(utf, strDesGbk);
	printf("%s", strDesGbk);
}

/// <summary>
/// 从返回消息中获取SessionId
/// </summary>
/// <param name="buf">返回消息</param>
/// <param name="sid">SessionId</param>
/// <returns></returns>
bool GetSessionId(char* responseMessage, char* sessionId)
{
	char* p = strstr(responseMessage + 20, "SessionId");
	if (p)
	{
		p += 12;
		char* pEnd = strstr(p, "\"");
		memcpy(sessionId, p, pEnd - p);
		_SessionLen = strlen(sessionId);
		//printf("session id '%s'\n", sessionId);
		return true;
	}
	return false;
}
/// <summary>
/// 获取UserId
/// </summary>
/// <param name="responseMessage">Response Message</param>
/// <param name="userId">User Id</param>
/// <returns></returns>
bool GetUserId(char* responseMessage, char* userId)
{
	char* p = strstr(responseMessage + 20, "UserId");
	if (p)
	{
		p += 9;
		char* pEnd = strstr(p, "\"");
		memcpy(userId, p, pEnd - p);
		//printf("user id '%s'\n", userId);
		return true;
	}
	return false;
}
/// <summary>
/// 解压数据，消息头和session之后的消息才需要解压
/// </summary>
/// <param name="dataBuffer"></param>
/// <param name="dataLen"></param>
/// <returns></returns>
char* myUncompress(char* dataBuffer, int dataLen)
{
	unsigned int newlen = 0;
	uLongf dlen = 920240;
	uLongf slen = dataLen - 0x26 - 8 - _SessionLen;
	memset(tem, 0, 920240);
	int nRet = uncompress((Bytef*)tem, &dlen, (Bytef*)dataBuffer + 8 + _SessionLen + 0x26, slen);
	char strDesGbk[920240] = { 0 };
	utf8ToGbk(tem, strDesGbk);
	if (nRet == 0)
	{
		return strDesGbk;
	}
	//去掉消息头,session,换行符
	return dataBuffer + 8 + _SessionLen + 0x26;
}

/// <summary>
/// 读取通信数据
/// </summary>
/// <param name="dataBuffer">读取数据缓冲区</param>
/// <param name="messageHeader">消息头</param>
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
/// 发送消息
/// </summary>
/// <param name="sendBuffer">Send Buffer</param>
/// <param name="readBuffer">Read Buffer</param>
/// <param name="message">Message</param>
/// <returns>消息长度</returns>
int mySendMessage(char* sendBuffer, char* readBuffer, char* message)
{
	//在函数退出时自动回收。所以此时无需释放
	struct Header messageHeader1;
	struct Header2 messageHeader2;
	//printf("send message:'%s'\n", message);
	memset(&messageHeader1, 0, sizeof(messageHeader1));
	messageHeader1.A = 0x80;
	messageHeader1.nNum1 = strlen(message) + 8 + _SessionLen;
	memset(&messageHeader2, 0, sizeof(messageHeader2));
	messageHeader2.nNum1 = 0x1001;
	messageHeader2.nNum2 = 0x2600 + _SessionLen;
	//0x26=&, 去掉最后一个占位符
	messageHeader2.nNum3 = (unsigned short int)strlen(message) - 0x26;
	messageHeader2.nNum4 = 0x0000;

	memset(sendBuffer, 0, sizeof(sendBuffer));
	memcpy(sendBuffer, &messageHeader2, 8);
	memcpy(sendBuffer + 8, _SessionId, _SessionLen);
	memcpy(sendBuffer + 8 + _SessionLen, message, strlen(message));

	int len1 = SSL_write(_SSL, &messageHeader1, sizeof(messageHeader1));
	if (len1 < 0)
	{
		printf("'%s'message Send failure ！Error code is %d，Error messages are '%s'\n", "Message header", errno, strerror(errno));
	}
	int len2 = SSL_write(_SSL, sendBuffer, messageHeader1.nNum1);
	if (len2 < 0)
	{
		printf("'%s'message Send failure ！Error code is %d，Error messages are '%s'\n", sendBuffer, errno, strerror(errno));
	}
	printf("send message success\n");
	memset(readBuffer, 0, sizeof(readBuffer));
	int dataLen = myReadData(readBuffer);
	if (dataLen < 0)
	{
		printf("read data failed");
	}
	//printf("message length is '%d'\n", dataLen);
	return dataLen;
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
	char sendBuffer[500] = { 0 };
	char readBuffer[1000] = { 0 };
	char login[1200] = { 0 };
	//string type = accountType ? "YSG" : "DEM";
	sprintf(login, "{%s}{\"Arguments\":{\"appType\":\"18\",\"containsMilSInQuotationTime\":false,\"descriptionInfo\":\"Version:1.0.0.3 System:Operating system version 5.1, corresponds to Windows XP CPU:x86_64\",\"isGuestIdentity\":false,\"language\":\"CHS\",\"loginID\":\"%s\",\"password\":\"%s\",\"sendQuotationInSsl\":false,\"validPathName\":\"%s\"},\"Method\":\"Login\"}\0", _UUID.data(), userName, password, accountType);
	int dataLen = mySendMessage(sendBuffer, readBuffer, login);
	if (dataLen > 0)
	{
		GetSessionId(readBuffer, _SessionId);
		GetUserId(readBuffer, _UserId);
		return readBuffer;
	}
	return 0;
}
/// <summary>
/// 设置初始化数据
/// </summary>
/// <param name="szName"></param>
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
void getData(TiXmlDocument anyXML, const char* node, const char* chaildNode, const char* sell, const char* buy, const char* id)
{
	const TiXmlNode* lpMapNode = NULL; //初始化
	lpMapNode = anyXML.RootElement()->IterateChildren(node, lpMapNode);
	if (lpMapNode)
	{

		const TiXmlNode* lpItemNode = NULL;
		while (lpItemNode = lpMapNode->IterateChildren(chaildNode, lpItemNode))
		{
			std::string strID = lpItemNode->ToElement()->Attribute("InstrumentId");
			if (strcmp(strID.data(), id) == 0)
			{


				std::string strSell = lpItemNode->ToElement()->Attribute(sell);
				std::string strBuy = lpItemNode->ToElement()->Attribute(buy);

				if (strcmp(strRMBID.data(), strID.data()) == 0)
				{
					strRMBpriceSell = strSell;
					strRMBpriceBuy = strBuy;
				}

				gbkPrint("---卖：");
				gbkPrint(strSell.data());
				gbkPrint("---买：");
				gbkPrint(strBuy.data());
				gbkPrint("\r\n");
				break;
			}
		}
	}
}
/// <summary>
/// 获取初始化数据
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetInitData()
{
	char sendBuffer[500] = { 0 };
	char readBuffer[MAXBUF] = { 0 };
	char getInitData[1000] = { 0 };
	//char getInitData[] = "{1f33f143-94d7-406e-8958-edfc7c34d3ae}{\"Arguments\":{}, \"Method\":\"GetInitData\"}\0";
	sprintf(getInitData, "{%s}{\"Arguments\":{}, \"Method\":\"GetInitData\"}\0", _UUID.data());
	int dataLen = mySendMessage(sendBuffer, readBuffer, getInitData);
	char* res = myUncompress(readBuffer, dataLen);
	if (strstr(tem, "InitializeData"))
	{
		InitDataXML.Parse(tem);
		const TiXmlNode* lpMapNode = NULL; //初始化
		lpMapNode = InitDataXML.RootElement()->IterateChildren("Instruments", lpMapNode);
		if (lpMapNode)
		{
			const TiXmlNode* lpItemNode = NULL;
			while (lpItemNode = lpMapNode->IterateChildren("Instrument", lpItemNode))
			{
				strDes = lpItemNode->ToElement()->Attribute("Description");
				std::string strInstrumentID = lpItemNode->ToElement()->Attribute("Id");
				if (strstr(strInstrumentID.data(), "55f6a2b3-51a8"))
				{
					strRMBID = strInstrumentID;// strRMBpriceSell
				}
				gbkPrint("------------------------------------------------------\r\n");
				gbkPrint(strDes.data());
				getData(InitDataXML, "Quotations", "Quotation", "Bid", "Ask", strInstrumentID.data());
			}
		}
	}
	return res;
}
/// <summary>
/// 获取初始化设置数据
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetSettingData()
{
	char sendBuffer[500] = { 0 };
	char readBuffer[MAXBUF] = { 0 };
	char getSettingData[1000] = { 0 };
	//char getSettingData[] = "{c3ca837b-2a0c-4821-aad4-5bd32eff2685}{\"Arguments\":{}, \"Method\":\"GetSettingData\"}\0";
	sprintf(getSettingData, "{%s}{\"Arguments\":{}, \"Method\":\"GetSettingData\"}\0", _UUID.data());
	int dataLen = mySendMessage(sendBuffer, readBuffer, getSettingData);
	char* res = myUncompress(readBuffer, dataLen);
	return res;
}
/// <summary>
/// 获取报价
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetChartQuotaion()
{
	char sendBuffer[500] = { 0 };
	char readBuffer[500] = { 0 };
	char getChartQuotaion[1000] = { 0 };
	char nowtime[30] = { 0 };
	getNowTime2(nowtime);
	//char getSettingData[] = "{c3ca837b-2a0c-4821-aad4-5bd32eff2685}{\"Arguments\":{}, \"Method\":\"GetSettingData\"}\0";
	sprintf(getChartQuotaion, "{%s}{\"Arguments\":{\"accountID\":\"\",\"dataCycleParameter\":\"M1\",\"from\":\"%s\",\"instrumentID\":\"%s\",\"quotePolicyID\":\"%s\",\"rangeType\":0,\"requestType\":1,\"to\":\"%s\"},\"Method\":\"AsyncGetChartData2ForCppTrader\"}", _UUID.data(), nowtime, _InstrumentId, _QuotePolicyId, nowtime);
	int dataLen = mySendMessage(sendBuffer, readBuffer, getChartQuotaion);
	char* res = myUncompress(readBuffer, dataLen);
	return res;
}
/// <summary>
/// 获取Bid Price
/// </summary>
/// <param name="buf"></param>
/// <returns></returns>
double GetBidPrice(char* buf)
{
	char* p = strstr(buf, "Open");
	if (p)
	{
		memset(_BidPrice, 0, 100);
		p += 6;
		char* pEnd = strstr(p, "\"");
		memcpy(_BidPrice, p, pEnd - p);
		return atof(_BidPrice);
	}
	return NULL;
}
/// <summary>
/// 获取Ask Price
/// </summary>
/// <param name="buf"></param>
/// <returns></returns>
double GetAskPrice(char* buf)
{
	char* p = strstr(buf, "Close");
	if (p)
	{
		memset(_AskPrice, 0, 100);
		p += 7;
		char* pEnd = strstr(p, "\"");
		memcpy(_AskPrice, p, pEnd - p);
		return atof(_AskPrice);
	}
	return NULL;
}
/// <summary>
/// 开仓买多
/// </summary>
/// <param name="lot"></param>
/// <param name="price"></param>
/// <param name="slippage"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* OpenBuyOrder(char* price, char* slippage, char* lot)
{
	char sendBuffer[1200] = { 0 };
	char readBuffer[MAXBUF] = { 0 };
	double priced = atof(price);
	double slippaged = atof(slippage);
	if (priced == 0)
	{
		char* instrumentIDBuff = GetChartQuotaion();
		priced = GetBidPrice(instrumentIDBuff) + slippaged;
	}
	else
	{
		priced = priced + slippaged;
	}
	char pp2[20];
	sprintf(pp2, "%.2f", priced);
	char openBuy[900] = { 0 };
	char nowtime[30] = { 0 };
	getNowTime(nowtime);
	//char getSettingData[] = "{c3ca837b-2a0c-4821-aad4-5bd32eff2685}{\"Arguments\":{}, \"Method\":\"GetSettingData\"}\0";
	sprintf(openBuy, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"true\\\" Lot=\\\"%s\\\" IsBuy=\\\"true\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\"/>\\n</Transaction>\\n\"},\"Method\":\"Place\"}", _UUID.data(), _InstrumentId, _UUID.data(), nowtime, _AccountId, nowtime, _UserId, lot, pp2, _UUID.data(), lot);
	int dataLen = mySendMessage(sendBuffer, readBuffer, openBuy);
	char* res = myUncompress(readBuffer, dataLen);
	return res;
}
/// <summary>
/// 开仓卖空
/// </summary>
/// <param name="lot"></param>
/// <param name="price"></param>
/// <param name="slippage"></param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* OpenSellOrder(char* price, char* slippage, char* lot)
{
	char sendBuffer[1200] = { 0 };
	char readBuffer[MAXBUF] = { 0 };
	double priced = atof(price);
	double slippaged = atof(slippage);
	if (priced == 0)
	{
		char* instrumentIDBuff = GetChartQuotaion();
		priced = GetAskPrice(instrumentIDBuff) + slippaged;
	}
	else
	{
		priced = priced + slippaged;
	}
	char pp2[20];
	sprintf(pp2, "%.2f", priced);
	char openSell[900] = { 0 };
	char nowtime[30] = { 0 };
	getNowTime(nowtime);
	sprintf(openSell, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"true\\\" Lot=\\\"%s\\\" IsBuy=\\\"false\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\"/>\\n</Transaction>\\n\"}, \"Method\":\"Place\"}", _UUID.data(), _InstrumentId, _UUID.data(), nowtime, _AccountId, nowtime, _UserId, lot, pp2, _UUID.data(), lot);
	int dataLen = mySendMessage(sendBuffer, readBuffer, openSell);
	char* res = myUncompress(readBuffer, dataLen);
	return res;
}

/// <summary>
/// 平仓
/// </summary>
/// <param name="nowPrice">平仓价格</param>
/// <param name="slippage">滑点</param>
/// <param name="openPrice">开仓价格</param>
/// <param name="lot">手数</param>
/// <param name="orderID">订单ID</param>
/// <param name="openTime">开仓时间</param>
/// <param name="isBuy">订单类型True=买多开仓,False=卖空开仓</param>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* CloseOrder(char* nowPrice, char* slippage, char* openPrice, char* lot, char* orderID, char* openTime, char* isBuy)
{
	char sendBuffer[1200] = { 0 };
	char readBuffer[MAXBUF] = { 0 };
	double nowPriced = atof(nowPrice);
	double slippaged = atof(slippage);
	if (nowPriced == 0)
	{
		char* instrumentIDBuff = GetChartQuotaion();
		if (isBuy == "True")
		{
			nowPriced = GetAskPrice(instrumentIDBuff) - slippaged;
		}
		else
		{
			nowPriced = GetBidPrice(instrumentIDBuff) + slippaged;
		}
	}
	else
	{
		if (isBuy == "True")
		{
			nowPriced = nowPriced - slippaged;
		}
		else
		{
			nowPriced = nowPriced + slippaged;
		}
	}
	char pp[20];
	sprintf(pp, "%.2f", nowPriced);
	char closeOrder[1000] = { 0 };
	char nowTime[30] = { 0 };
	memset(nowTime, 0, 30);
	getNowTime(nowTime);
	char szBuyAll[1000] = { 0 };
	sprintf(closeOrder, "{%s}{\"Arguments\":{\"transactionXml\":\"<Transaction EndTime=\\\"1800-01-01 00:00:00\\\" OrderType=\\\"0\\\" PlacingSettingType=\\\"0\\\" ExpireType=\\\"0\\\" InstrumentID=\\\"%s\\\" OrganizationCode=\\\"\\\" ID=\\\"%s\\\" SubType=\\\"0\\\" BeginTime=\\\"%s\\\" AccountID=\\\"%s\\\" SubmitTime=\\\"%s\\\" Type=\\\"0\\\" SubmitorID=\\\"%s\\\" ExpireTime=\\\"\\\">\n <Order OriginalLot=\\\"%s\\\" SetPrice=\\\"%s\\\" ID=\\\"%s\\\" IsOpen=\\\"false\\\" Lot=\\\"%s\\\" IsBuy=\\\"%s\\\" DQMaxMove=\\\"0\\\" TradeOption=\\\"0\\\" SetPrice2=\\\"\\\" PhysicalTradeSide=\\\"0\\\">\n  <OrderRelation OpenOrderTime=\\\"%s\\\" OpenOrderID=\\\"%s\\\" OpenOrderPrice=\\\"%s\\\" ClosedLot=\\\"%s\\\"/>\n </Order>\\n</Transaction>\\n\"}, \"Method\":\"Place\"}",
		_UUID.data(), _InstrumentId, _UUID.data(), nowTime, _AccountId, nowTime, _UserId, lot, openPrice, _UUID.data(), lot, isBuy, openTime, orderID, pp, lot);
	int dataLen = mySendMessage(sendBuffer, readBuffer, closeOrder);
	char* res = myUncompress(readBuffer, dataLen);
	return res;
}
/// <summary>
/// 获取订单,账户最新数据
/// </summary>
/// <returns></returns>
extern "C" __declspec(dllexport)
char* GetTradingData()
{
	char sendBuffer[1200] = { 0 };
	char readBuffer[MAXBUF] = { 0 };
	char getTradingData[500] = { 0 };
	char nowtime[30] = { 0 };
	getNowTime(nowtime);
	sprintf(getTradingData, "{%s}{\"Arguments\":{\"ids\":[\"%s\"] }, \"Method\" : \"GetTradingData\"}", _UUID.data(), _AccountId);
	int dataLen = mySendMessage(sendBuffer, readBuffer, getTradingData);
	char* res = myUncompress(readBuffer, dataLen);
	return res;
}

int main(int argc, char** argv)
{

	char ip[] = "203.160.75.182";
	char port[] = "4523";
	if (Connect(ip, port))
	{
		printf("connection success\n");
	}
	else
	{
		printf("connection failed\n");
	}
	GetUUID(_UUID);
	printf(_UUID.data());
	printf("\n");

	char userId[100] = "35037";
	char password[100] = "ab168168";
	char accountType[10] = "DEM";
	//sprintf(szLogin2, "{%s}{\"Arguments\":{\"appType\":\"18\",\"containsMilSInQuotationTime\":false,\"descriptionInfo\":\"Version:1.0.0.3 System:Operating system version 5.1, corresponds to Windows XP CPU:x86_64\",\"isGuestIdentity\":false,\"language\":\"CHS\",\"loginID\":\"%s\",\"password\":\"%s\",\"sendQuotationInSsl\":false,\"validPathName\":\"DEM\"},\"Method\":\"Login\"}\0", _UUID.data(), "35037", "ab168168");
	char* res = Login(userId, password, accountType);
	printf("session is '%s'\n", _SessionId);
	printf("session lengh is '%d'\n", strlen(_SessionId));
	printf("user is '%s'\n", _UserId);
	if (_SessionLen > 0)
	{
		//消息发送缓冲区
		char sendBuffer2[500] = { 0 };
		//消息接收缓冲区
		char readBuffer2[120240] = { 0 };
		char* len1 = GetInitData();
		printf("\n");
		//gbkPrint(len1);
		char accountId[] = "ba756498-d979-4109-9905-43d4abc3916c";
		char instrumentId[] = "55f6a2b3-51a8-466a-99d2-45897bbf13fb";
		char quotePolicyId[] = "4c1dba2d-6007-4df3-93cd-6cf61afd0332";
		//gbkPrint(len1);
		SetInitParas(accountId, instrumentId, quotePolicyId);
		char* len2 = GetSettingData();
		//gbkPrint(len2);
		printf("\n");
		printf("SettingData success\n");

		//////////////////////////////////////////////////////////////////买入1手
		gbkPrint("\r\n\r\n------------------开始买入一手------------------\r\n");
		//消息发送缓冲区
		char sendBuffer4[500] = { 0 };
		//消息接收缓冲区
		char readBuffer4[500] = { 0 };
		//平台Close后不能获取数据，返回异常
		char* instrumentIDBuff = GetChartQuotaion();
		printf("\n");
		double ask = GetAskPrice(instrumentIDBuff);
		printf("ask = %lf\n", ask);
		double bid = GetBidPrice(instrumentIDBuff);
		printf("bid = %lf\n", bid);
		char slippage[] = "0.05";
		char askPrice[10] = { 0 };
		sprintf(askPrice, "%.2f", ask);
		char bidPrice[10] = { 0 };
		sprintf(bidPrice, "%.2f", bid);
		//消息发送缓冲区
		char sendBuffer5[MAXBUF] = { 0 };
		//消息接收缓冲区
		char readBuffer5[MAXBUF] = { 0 };
		//char szInstrumentID[] = "55f6a2b3-51a8-466a-99d2-45897bbf13fb";
		char szLot[] = "1";
		//char szPrice[] = "1789.5";
		//char* buy = OpenBuyOrder(szLot, bidPrice, slippage);
		printf("open buy success\n");
		//printf(buy);
		printf("\n");

		//char* sell = OpenSellOrder(szLot, askPrice, slippage);
		printf("open sell success\n");
		//printf(sell);
		printf("\n");

		//char* orders = GetTradingData();
		printf("get trading data success\n");
		//printf(orders);
		printf("\n");
		//消息发送缓冲区
		char sendBuffer8[MAXBUF] = { 0 };
		//消息接收缓冲区
		char readBuffer8[MAXBUF] = { 0 };
		char orderId[] = "fd377bd1-c71b-40ee-a3c7-783f04fd81d7";//Order/ID
		char IsBuy[] = "True";//卖空单/买多单//Order/IsBuy
		char IsOpen[] = "True";//开仓/平仓//Order/IsOpen
		char Lot[] = "0.1";//Order/Lot
		char ExecutePrice[] = "1734.50";//Order/ExecutePrice
		char UpdateTime[] = "2021-03-16 12:53:46";//Transaction/SubmitTime
		char closePrice[10] = { 0 };
		char* closeres = CloseOrder(closePrice, slippage, ExecutePrice, Lot, orderId, UpdateTime, IsBuy);
		printf("close order success\n");
		printf(closeres);
		printf("\n");
	}
	else
	{
		printf("login failed\n");
	}
	printf("\n");
	DisConnection();
	system("PAUSE");
	return 0;
}