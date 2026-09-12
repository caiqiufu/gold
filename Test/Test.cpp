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

#define MAXBUF 120240
char buffer[MAXBUF] = { 0 };
char readbuffer[MAXBUF] = { 0 };
char tem[920240] = { 0 };
char UserId[100] = { 0 };
TiXmlDocument InitDataXML;
TiXmlDocument SettingDataXML;
TiXmlDocument TradingDataXML;

//订单交易所需信息
std::string strRMBID;
std::string strRMBpriceSell;
std::string strRMBpriceBuy;
std::string strDes;
std::string strAccountId;

//平仓卖数据
std::string strInstrumentIDX;
std::string strTimeX;
std::string strOrderIDX;
std::string strLotX;
std::string strSetPriceX;
std::string strCodeX;

//平仓买数据
std::string strInstrumentIDY;
std::string strTimeY;
std::string strOrderIDY;
std::string strLotY;
std::string strSetPriceY;
std::string strCodeY;
//OpenSSL的决大部分API函数都在围绕这两种数据结构体完成TLS握手和数据加解密工作.
SSL_CTX* ctx;
//SSL 通信
SSL* ssl;
//socket监听套接字
int sockfd;
//结构体用来处理网络通信的地址
struct sockaddr_in dest;


//数据包头1
struct Header
{
	unsigned char A;//1个字节
	bool bflag;//1个字节
	unsigned short int nNum1;//4个字节  整个消息的长度
	unsigned short int nNum2;//4个字节
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
/// 获取字符串
/// </summary>
/// <param name="nowTime"></param>
/// <returns></returns>
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
/// 
/// </summary>
/// <param name="anyXML"></param>
/// <param name="node"></param>
/// <param name="chaildNode"></param>
/// <param name="sell"></param>
/// <param name="buy"></param>
/// <param name="id"></param>
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
/// 打印字符串
/// </summary>
/// <param name="buf">源字符</param>
/// <param name="len"></param>
void WriteF(char* buf, int len)
{
	//FILE* fd = fopen("d://a.txt", "ab+");
	unsigned int newlen = 0;
	//memcpy指的是C和C++使用的内存拷贝函数
	memcpy(&newlen, buf + 8 + nSessionLen + 0x26, 4);
	uLongf dlen = 920240;
	uLongf slen = len - 0x26 - 8 - nSessionLen;
	memset(tem, 0, 920240);
	printf("message length is '%d'\n", len);
	printf("nSessionLen length is '%d'\n", nSessionLen);
	printf("slen length is '%d'\n", slen);
	printf("buf length is '%d'\n", strlen(buf));
	int nRet = uncompress((Bytef*)tem, &dlen, (Bytef*)buf + 8 + nSessionLen + 0x26, slen);
	//fwrite(tem, dlen, 1, fd);	
	if (nRet == 0)
	{
		if (strstr(tem, "InitializeData"))
		{
			gbkPrint(tem);
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
					if (strstr(strInstrumentID.data(), "0c4188c7"))
					{
						strRMBID = strInstrumentID;// strRMBpriceSell
					}
					gbkPrint("------------------------------------------------------\r\n");
					gbkPrint(strDes.data());
					getData(InitDataXML, "Quotations", "Quotation", "Bid", "Ask", strInstrumentID.data());
				}
			}

		}
		if (strstr(tem, "CurrencyCode"))
		{
			gbkPrint(tem);
			TradingDataXML.Parse(tem);
			const TiXmlNode* lpItemNode = NULL;//初始化
			lpItemNode = TradingDataXML.RootElement()->IterateChildren("Account", lpItemNode);
			strAccountId = lpItemNode->ToElement()->Attribute("ID");
			std::string strName = lpItemNode->ToElement()->Attribute("CustomerName");
			std::string strBalance = lpItemNode->ToElement()->Attribute("Balance");
			std::string strTradePLFloat = lpItemNode->ToElement()->Attribute("TradePLFloat");
			std::string strEquity = lpItemNode->ToElement()->Attribute("Equity");
			std::string strHedgeNecessary = lpItemNode->ToElement()->Attribute("HedgeNecessary");
			double dbEquity = atof(strEquity.data());
			double dbHedgeNecessary = atof(strHedgeNecessary.data());
			double fff = dbEquity - dbHedgeNecessary;
			char cff[60] = { 0 };
			sprintf(cff, "%f", fff);
			gbkPrint("\r\n\r\n------------------开始打印订单信息------------------\r\n");
			const TiXmlNode* lpMapNode = NULL; //初始化
			lpMapNode = TradingDataXML.RootElement()->IterateChildren("Account", lpMapNode)->IterateChildren("Transacti", lpMapNode);
			if (lpMapNode)
			{
				const TiXmlNode* lpItemNode = NULL;
				const TiXmlNode* lpItemNode3 = NULL;
				while (lpItemNode = lpMapNode->IterateChildren("Transaction", lpItemNode))
				{
					const TiXmlNode* lpItemNode2 = NULL;
					std::string strInstrumentID = lpItemNode->ToElement()->Attribute("InstrumentID");
					std::string strTime = lpItemNode->ToElement()->Attribute("SubmitTime");
					if (strcmp("822636d4-98d0-4edc-be16-1a665f6c85d9", strInstrumentID.data()) == 0)
					{
						gbkPrint("本地倫敦銀:");
					}
					else if (strcmp("0c4188c7-376a-446d-b485-462f37c58536", strInstrumentID.data()) == 0)
					{
						gbkPrint("人民幣公斤條:");
					}
					else if (strcmp("55f6a2b3-51a8-466a-99d2-45897bbf13fb", strInstrumentID.data()) == 0)
					{
						gbkPrint("本地倫敦金:");
					}
					else
					{
						gbkPrint("其他:");
					}
					gbkPrint("---订单时间:");
					gbkPrint(strTime.data());
					lpItemNode2 = lpItemNode->IterateChildren("Orders", lpItemNode2)->IterateChildren("Order", lpItemNode2);
					std::string strOrderID = lpItemNode2->ToElement()->Attribute("ID");
					std::string strLot = lpItemNode2->ToElement()->Attribute("Lot");
					std::string strSetPrice = lpItemNode2->ToElement()->Attribute("SetPrice");
					std::string strCode = lpItemNode2->ToElement()->Attribute("Code");
					std::string strIsBuy = lpItemNode2->ToElement()->Attribute("IsBuy");
					gbkPrint("---订单手数:");
					gbkPrint(strLot.data());
					gbkPrint("---订单价格:");
					gbkPrint(strSetPrice.data());
					gbkPrint("---买卖:");
					if (strcmp(strIsBuy.data(), "True") == 0)
					{
						strInstrumentIDX = strInstrumentID;
						char temp[20] = { 0 };
						memcpy(temp, strTime.data(), 10);
						strTimeX = temp;
						strOrderIDX = strOrderID;
						strLotX = strLot;
						strSetPriceX = strSetPrice;
						strCodeX = strCode;
						gbkPrint("买\r\n");
					}
					else
					{
						strInstrumentIDY = strInstrumentID;
						char temp[20] = { 0 };
						memcpy(temp, strTime.data(), 10);
						strTimeY = temp;
						strOrderIDY = strOrderID;
						strLotY = strLot;
						strSetPriceY = strSetPrice;
						strCodeY = strCode;
						gbkPrint("卖\r\n");
					}

				}
			}
			gbkPrint("\r\n\r\n------------------开始打印账户信息------------------\r\n");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("账户名：");
			gbkPrint(strName.data());
			gbkPrint("\r\n");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("货币：");
			gbkPrint(strBalance.data());
			gbkPrint("\r\n：");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("结余：");
			gbkPrint(strBalance.data());
			gbkPrint("\r\n");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("浮动盈亏：");
			gbkPrint(strTradePLFloat.data());
			gbkPrint("\r\n");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("有效保证金：");
			gbkPrint(strEquity.data());
			gbkPrint("\r\n");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("所需保证金：");
			gbkPrint(strHedgeNecessary.data());
			gbkPrint("\r\n");
			gbkPrint("------------------------------------------------------\r\n");
			gbkPrint("可用保证金：");
			gbkPrint(cff);
			gbkPrint("\r\n");
		}
	}
}

/// <summary>
/// 读取通信数据
/// </summary>
/// <param name="buffer"></param>
/// <returns></returns>
int readData(char* buffer)
{
	int nRead = 0;
	int len = 0;
	/*
	将某一块内存中的内容全部设置为指定的值， 这个函数通常为新申请的内存做初始化工作
	void *memset(void *s, int ch, size_t n);
	将s中当前位置后面的n个字节 （typedef unsigned int size_t ）用 ch 替换并返回 s。
	*/
	memset(&myHeader, 0, sizeof(myHeader));
	//读取通信数据
	SSL_read(ssl, &myHeader, sizeof(myHeader));
	while (nRead < myHeader.nNum1)
	{
		len = SSL_read(ssl, buffer + nRead, myHeader.nNum1 - nRead);
		nRead += len;
		if (len <= 0) {
			break;
		}
	}
	WriteF(buffer, nRead);
	return nRead;
}
/// <summary>
/// 写入通信数据
/// </summary>
/// <param name="buffer"></param>
/// <param name="nLen"></param>
/// <returns></returns>
int  writeData(char* buffer, int nLen)
{
	//写入通信数据
	int len = SSL_write(ssl, buffer, nLen);
	if (len > 0) {
		printf("消息发送成功！\n");
	}
	return nLen;
}
/// <summary>
/// 显示通信证书信息
/// </summary>
/// <param name="ssl">SSL加密的安全数据传输通道</param>
void ShowCerts(SSL* ssl)
{
	//X.509 是密码学里公钥证书的格式标准
	X509* cert;
	char* line;
	//获取证书
	cert = SSL_get_peer_certificate(ssl);
	if (cert != NULL) {
		//使用给定的模式 mode 打开 filename 所指向的文件,对于wb+ 如果文件不存在则会建立，如果文件存在 会覆盖
		FILE* fd = fopen("d://a.pem", "wb+");
		PEM_write_X509(fd, cert);
		//i2d_X509_fp(fd, cert);
		fclose(fd);
		printf("数字证书信息:\n");
		//提取证书主题
		line = X509_NAME_oneline(X509_get_subject_name(cert), 0, 0);
		printf("证书: %s\n", line);
		//释放之前调用 calloc、malloc 或 realloc 所分配的内存空间
		free(line);
		//提取证书颁发者
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
/// <param name="buf">源字符串</param>
/// <param name="sid">生成的SessionId</param>
/// <returns></returns>
bool GetSessionId(char* buf, char* sid)
{
	printf("response is '%s'", buf);
	char* p = strstr(buf + 20, "SessionId");
	if (p)
	{
		p += 12;
		char* pEnd = strstr(p, "\"");
		memcpy(sid, p, pEnd - p);
		printf("session is '%s'", sid);
		return true;
	}
	return false;
}

/// <summary>
/// 获取UserId
/// </summary>
/// <param name="buf"></param>
/// <param name="sid"></param>
/// <returns></returns>
bool GetUserId(char* buf, char* sid)
{
	char* p = strstr(buf + 20, "UserId");
	if (p)
	{
		p += 9;
		char* pEnd = strstr(p, "\"");
		memcpy(sid, p, pEnd - p);
		return true;
	}
	return false;
}

/// <summary>
/// 连接
/// </summary>
/// <returns></returns>
bool myConnect()
{
	//OpenSSL初始化
	SSL_library_init();
	//密钥有经过口令加密需要这个函数
	OpenSSL_add_all_algorithms();
	//错误信息初始化
	SSL_load_error_strings();
	//申请SSL会话环境
	ctx = SSL_CTX_new(TLSv1_client_method());//SSLv23_client_method
	//生成一个TCP的socket,Function: int socket (int namespace, int style, int protocol)
	if ((sockfd = socket(AF_INET, SOCK_STREAM, 0)) < 0) {
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
	if (connect(sockfd, (struct sockaddr*)&dest, sizeof(dest)) != 0) {
		//	perror("Connect ");
		return false;
	}
	//printf("connectd server successly\n");
	/*基于 ctx 产生一个新的 SSL*/
	ssl = SSL_new(ctx);
	/*将连接用户的 socket 加入到 SSL*/
	SSL_set_fd(ssl, sockfd);
	/*建立 SSL 连接*/
	if (SSL_connect(ssl) == -1) {
		unsigned long ulErr = ERR_get_error(); // 获取错误号
		char szErrMsg[1024] = { 0 };
		char* pTmp = NULL;
		pTmp = ERR_error_string(ulErr, szErrMsg);
		//	printf(szErrMsg);
		//	printf(pTmp);
		return false;
	}
	return true;
}
/// <summary>
/// 关闭连接
/// </summary>
void myClose()
{
	SSL_shutdown(ssl);
	SSL_free(ssl);
	closesocket(sockfd);
	SSL_CTX_free(ctx);
}

int main(int argc, char** argv)
{


	std::string uuid;
	/*
	* 登录参数
	* appType：18
	* containsMilSInQuotationTime：false
	* isGuestIdentity:false
	* language:CHS
	* loginID: user code
	* password: password
	* sendQuotationInSsl:false
	* validPathName":"DEM"  DEM是模拟 YSG是真实
	*/
	const char* szLogin = "{95c8ecs4-6dad-4501-a6cb-7c6877318012}{\"Arguments\":{\"appType\":\"18\",\"containsMilSInQuotationTime\":false,\"descriptionInfo\":\"Version:1.0.0.3 System:Operating system version 5.1, corresponds to Windows XP CPU:x86_64\",\"isGuestIdentity\":false,\"language\":\"CHS\",\"loginID\":\"05188\",\"password\":\"ab168168\",\"sendQuotationInSsl\":false,\"validPathName\":\"DEM\"},\"Method\":\"Login\"}\0";
	/*
	* 获取初始化数据的请求字符串
	*
	*/
	const char* szGetInitData = "{1f33f143-94d7-406e-8958-edfc7c34d3ae}{\"Arguments\":{}, \"Method\":\"GetInitData\"}\0";
	/*
	* 获取设置数据的字符串
	*/
	const char* szGetSettingData = "{c3ca837b-2a0c-4821-aad4-5bd32eff2685}{\"Arguments\":{}, \"Method\":\"GetSettingData\"}\0";
	/*
	* 获取交易数据
	*
	*/
	const char* szGetTradingData = "{6592ab06-4207-4261-80ea-8db921455e97}{\"Arguments\":{\"ids\":[\"ab9549b4-87b9-403c-b521-4a0da138c352\"] }, \"Method\" : \"GetTradingData\"}\0";

	char SessionId[5] = { 0 };
	char szSell[6000] = { 0 };
	char szBuy[6000] = { 0 };
	char szBuyAll[6000] = { 0 };
	char szSellAll[6000] = { 0 };
	char szLogin2[1200] = { 0 };
	char user[50] = { 0 };
	char pass[50] = { 0 };
	char qd[255] = { 0 };
	char time[30] = { 0 };


	memset(&dest, 0, sizeof(dest));
	dest.sin_family = AF_INET;
	//登录端口
	dest.sin_port = htons(4523);
	//登录IP
	dest.sin_addr.s_addr = inet_addr("203.160.75.182");
	//创建一个被指定变量连接而成的WORD变量。返回一个WORD变量。
	WORD ver = MAKEWORD(2, 2);
	//被WSAStartup函数调用后返回的 Windows Sockets数据
	WSADATA dat;
	//wsastartup主要就是进行相应的socket库绑定
	WSAStartup(ver, &dat);

	if (!myConnect())
	{
		myClose();
		return NULL;
	}
	sprintf(szLogin2, "{%s}{\"Arguments\":{\"appType\":\"18\",\"containsMilSInQuotationTime\":false,\"descriptionInfo\":\"Version:1.0.0.3 System:Operating system version 5.1, corresponds to Windows XP CPU:x86_64\",\"isGuestIdentity\":false,\"language\":\"CHS\",\"loginID\":\"%s\",\"password\":\"%s\",\"sendQuotationInSsl\":false,\"validPathName\":\"DEM\"},\"Method\":\"Login\"}\0", GetUUID(uuid).data(), "35037", "ab168168");
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
	SSL_write(ssl, &myHeader, sizeof(myHeader));
	SSL_write(ssl, buffer, myHeader.nNum1);
	memset(readbuffer, 0, MAXBUF);
	int nRead = readData(readbuffer + 8 + nSessionLen);



	if (GetSessionId(readbuffer, SessionId))
	{
		gbkPrint("------------------开始打印行情数据------------------\r\n\r\n");
		GetUserId(readbuffer, UserId);
		nSessionLen = strlen(SessionId);
		memset(&myHeader, 0, sizeof(myHeader));
		myHeader.A = 0x80;//0x表示该数据时16进制，80=128
		myHeader.nNum1 = strlen(szGetInitData) + 8 + nSessionLen;

		memset(&myHeader2, 0, sizeof(myHeader2));
		myHeader2.nNum1 = 0x1001;//4097
		myHeader2.nNum2 = 0x2600 + nSessionLen;//9728
		myHeader2.nNum3 = (unsigned short int)strlen(szGetInitData) - 0x26;
		myHeader2.nNum4 = 0x0000;

		memcpy(buffer, &myHeader2, 8);
		memcpy(buffer + 8, SessionId, nSessionLen);
		memcpy(buffer + 8 + nSessionLen, szGetInitData, strlen(szGetInitData));
		SSL_write(ssl, &myHeader, sizeof(myHeader));
		SSL_write(ssl, buffer, myHeader.nNum1);
		memset(readbuffer, 0, MAXBUF);
		nRead = readData(readbuffer);

	}
	else
	{
		gbkPrint("用户名或密码错误!!!!!!!\r\n");
	}


	myClose();
	system("PAUSE");
	return 0;
}