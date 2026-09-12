import requests
from bs4 import BeautifulSoup
import re


# 登录认证，获取access_token
def get_token():
    url = "https://api.ma288.com/members/oauth/token"
    # 请求参数中的username、password需自行修改，d参数大概率是随机值
    payload = 'grant_type=password&username=A8&password=16881688&d=0.9503883153227932'
    headers = {
        'sec-ch-ua-platform': '"Windows"',
        'Authorization': 'Basic bWEyODhyYWRpbzptYTI4OG1hMjg4bWEyODg=',
        'Cache-Control': 'no-cache',
        'Referer': 'https://www.ma288.com/',
        'sec-ch-ua': '"Not(A:Brand";v="99", "Microsoft Edge";v="133", "Chromium";v="133"',
        'sec-ch-ua-mobile': '?0',
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36 Edg/133.0.0.0',
        'Accept': 'application/json, text/plain, */*',
        'Content-Type': 'application/x-www-form-urlencoded',
        'Cookie': 'locale=zh_hk'
    }
    response = requests.request("POST", url, headers=headers, data=payload)
    return response.json().get("access_token")


# 获取最新页面
def get_first_page(access_token):
    url = "https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getPersonalPage.do?raceNo=0&displayType=5"
    payload = {}
    headers = {
        'accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7',
        'accept-language': 'zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6',
        'cache-control': 'no-cache',
        'pragma': 'no-cache',
        'priority': 'u=0, i',
        'referer': 'https://www.ma288.com/has/zh_TW/home/index.do',
        'sec-ch-ua': '"Not(A:Brand";v="99", "Microsoft Edge";v="133", "Chromium";v="133"',
        'sec-ch-ua-mobile': '?0',
        'sec-ch-ua-platform': '"Windows"',
        'sec-fetch-dest': 'document',
        'sec-fetch-mode': 'navigate',
        'sec-fetch-site': 'same-origin',
        'sec-fetch-user': '?1',
        'upgrade-insecure-requests': '1',
        'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36 Edg/133.0.0.0',
        'Cookie': f'locale=zh_hk; auth_token={access_token};'
    }
    response = requests.request("GET", url, headers=headers, data=payload)
    html = response.text
    print("get_first_page:"+html)
    # 正则表达式模式
    pattern_raceId = r'var\s+raceId\s*=\s*(\d+)'
    pattern_currentRaceDate = r"var\s+currentRaceDate\s*=\s*'(\d{2}/\d{2}/\d{4})'"

    # 查找匹配项
    match_raceId = re.search(pattern_raceId, html)
    match_currentRaceDate = re.search(pattern_currentRaceDate, html)

    # 提取变量和值
    raceId = match_raceId.group(1) if match_raceId else None
    currentRaceDate = match_currentRaceDate.group(1) if match_currentRaceDate else None

    # 输出结果
    # print(f"raceId: {raceId}")
    # print(f"currentRaceDate: {currentRaceDate}")

    return raceId, currentRaceDate


# 获取最新数据
def get_first_data(access_token, raceId, currentRaceDate):
    url = "https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getAllTicketsHTML.do"
    payload = f"noOfRow=15&raceId={raceId}&oddsType=WIN,PLA,QIN,QPL,DBL,TRI,LDBL-2,FCT&setting=sort1&decorator=blank&confirm=true"
    headers = {
    'accept': '*/*',
    'accept-language': 'zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6',
    'cache-control': 'no-cache',
    'content-type': 'application/x-www-form-urlencoded; charset=UTF-8',
    'origin': 'https://www.ma288.com',
    'pragma': 'no-cache',
    'priority': 'u=1, i',
    'referer': 'https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getPersonalPage.do?raceNo=0&displayType=5',
    'sec-ch-ua': '"Not(A:Brand";v="99", "Microsoft Edge";v="133", "Chromium";v="133"',
    'sec-ch-ua-mobile': '?0',
    'sec-ch-ua-platform': '"Windows"',
    'sec-fetch-dest': 'empty',
    'sec-fetch-mode': 'cors',
    'sec-fetch-site': 'same-origin',
    'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36 Edg/133.0.0.0',
    'x-requested-with': 'XMLHttpRequest',
    'Cookie': f'locale=zh_hk; auth_token={access_token}'
    }
    response = requests.request("POST", url, headers=headers, data=payload)
    html = response.text
    print("get_first_data:"+html)
    # 解析数据（python使用三方库beautifulsoup，如果是java可使用三方库jsoup）
    soup = BeautifulSoup(html, 'html.parser')
    data = []

    # 逆序遍历 table.nt 表格
    for table in reversed(soup.select('table.nt')):
        # 遍历表格中的 tr
        for tr in table.find_all('tr'):
            row = []
            # 逆序遍历 tr 中的 td，最多取前 4 个
            for td in list(reversed(tr.find_all('td')))[:4]:
                row.append(td.get_text(strip=True))
            data.append(row)

    return data    


# 获取历史页面
def get_history_page(access_token, raceDate):
    url = f"https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getHistoryOddsTrendByDate.do?raceDate={raceDate}&displayType=5"
    payload = {}
    headers = {
    'accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7',
    'accept-language': 'zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6',
    'cache-control': 'no-cache',
    'pragma': 'no-cache',
    'priority': 'u=0, i',
    'referer': 'https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getPersonalPage.do?raceNo=0&displayType=5',
    'sec-ch-ua': '"Not(A:Brand";v="99", "Microsoft Edge";v="133", "Chromium";v="133"',
    'sec-ch-ua-mobile': '?0',
    'sec-ch-ua-platform': '"Windows"',
    'sec-fetch-dest': 'document',
    'sec-fetch-mode': 'navigate',
    'sec-fetch-site': 'same-origin',
    'sec-fetch-user': '?1',
    'upgrade-insecure-requests': '1',
    'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36 Edg/133.0.0.0',
    'Cookie': f'locale=zh_hk; auth_token={access_token}'
    }
    response = requests.request("GET", url, headers=headers, data=payload)
    html = response.text
    print("get_history_page:"+html)
    # 正则表达式模式
    pattern_raceId = r'var\s+raceId\s*=\s*(\d+)'
    pattern_currentRaceDate = r"var\s+currentRaceDate\s*=\s*'(\d{2}/\d{2}/\d{4})'"

    # 查找匹配项
    match_raceId = re.search(pattern_raceId, html)
    match_currentRaceDate = re.search(pattern_currentRaceDate, html)

    # 提取变量和值
    raceId = match_raceId.group(1) if match_raceId else None
    currentRaceDate = match_currentRaceDate.group(1) if match_currentRaceDate else None

    # 输出结果
    # print(f"raceId: {raceId}")
    # print(f"currentRaceDate: {currentRaceDate}")

    return raceId, currentRaceDate


# 获取历史数据
def get_history_data(access_token, raceId, currentRaceDate):
    url = "https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getHistoryTicketsByRaceId.do"
    payload = f"noOfRow=15&raceId={raceId}&oddsType=WIN,PLA,QIN,QPL,DBL,TRI,LDBL-2,FCT&decorator=blank&confirm=true"
    headers = {
        'accept': '*/*',
        'accept-language': 'zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6',
        'cache-control': 'no-cache',
        'content-type': 'application/x-www-form-urlencoded; charset=UTF-8',
        'origin': 'https://www.ma288.com',
        'pragma': 'no-cache',
        'priority': 'u=1, i',
        'referer': f'https://www.ma288.com/has/zh_TW/odds/oddsTrendAction_getHistoryOddsTrendByDate.do?raceDate={currentRaceDate}&displayType=5',
        'sec-ch-ua': '"Not(A:Brand";v="99", "Microsoft Edge";v="133", "Chromium";v="133"',
        'sec-ch-ua-mobile': '?0',
        'sec-ch-ua-platform': '"Windows"',
        'sec-fetch-dest': 'empty',
        'sec-fetch-mode': 'cors',
        'sec-fetch-site': 'same-origin',
        'user-agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36 Edg/133.0.0.0',
        'x-requested-with': 'XMLHttpRequest',
        'Cookie': f'locale=zh_hk; auth_token={access_token};'
    }
    response = requests.request("POST", url, headers=headers, data=payload)
    html = response.text
    
    # 解析数据（python使用三方库beautifulsoup，如果是java可使用三方库jsoup）
    soup = BeautifulSoup(html, 'html.parser')
    data = []

    # 逆序遍历 table.nt 表格
    for table in reversed(soup.select('table.nt')):
        # 遍历表格中的 tr
        for tr in table.find_all('tr'):
            row = []
            # 逆序遍历 tr 中的 td，最多取前 4 个
            for td in list(reversed(tr.find_all('td')))[:4]:
                row.append(td.get_text(strip=True))
            data.append(row)

    return data


if __name__ == '__main__':
    # 获取access_token，有效期10分钟
    # 如果此处填写，则不去获取，直接使用，留空则去获取
    access_token = ""
    if not access_token:
        print("get_token ...")
        access_token = get_token()
        print(access_token)
        print()

    # 获取最新

    print("get_first_page ...")
    raceId, currentRaceDate = get_first_page(access_token)
    print(raceId, currentRaceDate)
    print()

    print("get_first_data ...")
    first_data = get_first_data(access_token, raceId, currentRaceDate)
    print(first_data)
    print()
    
    # 获取历史

    print("get_history_page ...")
    raceId, currentRaceDate = get_history_page(access_token, '19/02/2025')
    print(raceId, currentRaceDate)
    print()

    print("get_history_data ...")
    history_data = get_history_data(access_token, raceId, currentRaceDate)
    print(history_data)
    print()
