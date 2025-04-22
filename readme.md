# File Upload Service - ASP.NET Core Stream Upload

This is a sample project demonstrating how to implement stream file upload using ASP.NET Core. This service supports large file uploads, multipart form processing, and secure file type validation, using stream processing to prevent memory overflow issues.

## Features

- ✅ Stream file upload, supporting large file processing
- 🔒 File type whitelist validation
- 📦 Multipart form data processing
- 🚀 RESTful API design
- ⚡ Automatic directory creation and unique filename generation
- 🎯 Comprehensive error handling and logging

## Technology Stack

- **Backend Framework**: ASP.NET Core
- **File Processing**: Streaming
- **Language**: C#
- **Storage**: Local file system
- **Security**: File type validation, form field restrictions

## Quick Start

### Prerequisites

- .NET 6.0 or higher
- Local file system write permissions

### Installation Steps

1. **Clone the project**

git clone https://github.com/your-repo/stream-upload-service.git
cd stream-upload-service

2. **Restore NuGet packages**

dotnet restore

4. **Run the application**

dotnet run

## API Documentation

| Method | Endpoint                    | Description | Request Example     |
| ------ | --------------------------- | ----------- | ------------------- |
| POST   | /api/SteamUpload/UploadFile | Upload file | Multipart/form-data |

### Request Examples

Using curl to upload files:

curl -X POST
  https://localhost:7009/api/SteamUpload/UploadFile
  -H 'Content-Type: multipart/form-data'
  -F 'file=@/path/to/your/file.jpg'

Using Postman:

- Select POST method
- Set URL: /api/SteamUpload/UploadFile
- Choose Body → form-data
- Add key: file, type select File
- Select the file to upload

## Core Components Description

### SteamUploadController

Main upload controller, responsible for:

- Receiving file upload requests
- Calling upload service for processing
- Returning upload results

### LocalUploadService

Upload service implementation, responsible for:

- Creating upload directory structure
- Managing file storage process
- Handling upload exceptions

### FileStreamingHelper

Stream processing helper, responsible for:

- Parsing multipart form data
- Validating file type security
- Streaming file content writing

## Upload Process

1. **Request Validation** - Check if it's a valid multipart form request
2. **Directory Preparation** - Create UploadedFiles/{GUID}/ structure in project root directory
3. **Stream Processing** - Process form fields and files one by one
4. **File Validation** - Check if file extension is in the allowed list
5. **File Storage** - Save to disk using unique filename
6. **Result Return** - Return upload result information

## File Storage Structure

Uploaded files are stored in structured folders under the project root directory:

Project Root Directory/
├── UploadedFiles/
│   └── {GUID}/
│       ├── {GUID}_filename1.jpg
│       └── {GUID}_filename2.pdf

## Error Handling

The service provides comprehensive error handling mechanisms:

- Invalid content type (non multipart/form-data)
- Disallowed file extensions
- Form field count exceeding limits
- File stream processing exceptions
- Directory creation failure-

# 檔案上傳服務 - ASP.NET Core Stream Upload

這是一個展示如何使用 ASP.NET Core 實現串流檔案上傳的範例專案。該服務支援大檔案上傳、多部分表單處理和安全的檔案類型驗證，透過串流處理避免記憶體溢位問題。

## 功能特色

- ✅ 串流檔案上傳，支援大檔案處理
- 🔒 檔案類型白名單驗證
- 📦 多部分表單資料處理
- 🚀 RESTful API 設計
- ⚡ 自動目錄建立與唯一檔名生成
- 🎯 完整的錯誤處理與日誌記錄

## 技術棧

- **後端框架**: ASP.NET Core
- **檔案處理**: 串流處理 (Streaming)
- **語言**: C#
- **儲存**: 本地檔案系統
- **安全**: 檔案類型驗證、表單欄位限制

## 快速開始

### 前置需求

- .NET 6.0 或更高版本
- 本地檔案系統寫入權限

### 安裝步驟

1. **克隆專案**

git clone https://github.com/lauchiwai/stream_upload_file_dotnet.git
cd WebApplication1

2. **還原 NuGet 套件**

dotnet restore

4. **執行應用程式**

dotnet run

## API 文件

| 方法 | 端點                        | 描述     | 請求範例            |
| ---- | --------------------------- | -------- | ------------------- |
| POST | /api/SteamUpload/UploadFile | 上傳檔案 | Multipart/form-data |

### 請求範例

使用 curl 上傳檔案：

curl -X POST
  https://localhost:7009/api/SteamUpload/UploadFile
  -H 'Content-Type: multipart/form-data'
  -F 'file=@/path/to/your/file.jpg'

使用 Postman：

- 選擇 POST 方法
- 設定 URL: /api/SteamUpload/UploadFile
- 選擇 Body → form-data
- 新增 key: file，類型選擇 File
- 選擇要上傳的檔案

## 核心組件說明

### SteamUploadController

主要上傳控制器，負責：

- 接收檔案上傳請求
- 呼叫上傳服務處理
- 返回上傳結果

### LocalUploadService

上傳服務實現，負責：

- 建立上傳目錄結構
- 管理檔案儲存流程
- 處理上傳異常狀況

### FileStreamingHelper

串流處理助手，負責：

- 解析多部分表單資料
- 驗證檔案類型安全性
- 串流寫入檔案內容

## 上傳流程

1. **請求驗證** - 檢查是否為有效的多部分表單請求
2. **目錄準備** - 在專案根目錄建立 UploadedFiles/{GUID}/ 結構
3. **串流處理** - 逐個處理表單欄位和檔案
4. **檔案驗證** - 檢查檔案副檔名是否在允許列表中
5. **檔案儲存** - 使用唯一檔名儲存到磁碟
6. **結果返回** - 返回上傳結果資訊

## 檔案儲存結構

上傳的檔案會儲存在專案根目錄下的結構化資料夾中：

專案根目錄/
├── UploadedFiles/
│   └── {GUID}/
│       ├── {GUID}_filename1.jpg
│       └── {GUID}_filename2.pdf

## 錯誤處理

服務提供完整的錯誤處理機制：

- 無效的內容類型 (非 multipart/form-data)
- 不允許的檔案副檔名
- 表單欄位數量超過限制
- 檔案串流處理異常
- 目錄建立失敗-
