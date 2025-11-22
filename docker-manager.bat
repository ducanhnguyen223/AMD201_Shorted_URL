@echo off
echo ========================================
echo   URL Shortener - Docker Deployment
echo ========================================
echo.

:menu
echo Chon thao tac:
echo 1. Build tat ca services
echo 2. Khoi dong he thong
echo 3. Dung he thong
echo 4. Xem logs
echo 5. Kiem tra trang thai
echo 6. Reset toan bo (xoa database)
echo 7. Thoat
echo.
set /p choice="Nhap lua chon (1-7): "

if "%choice%"=="1" goto build
if "%choice%"=="2" goto start
if "%choice%"=="3" goto stop
if "%choice%"=="4" goto logs
if "%choice%"=="5" goto status
if "%choice%"=="6" goto reset
if "%choice%"=="7" goto end

echo Lua chon khong hop le!
goto menu

:build
echo.
echo Building tat ca services...
docker-compose build
echo.
echo Build hoan tat!
pause
goto menu

:start
echo.
echo Khoi dong he thong...
docker-compose up -d
echo.
echo Cho services khoi dong...
timeout /t 10 /nobreak > nul
echo.
echo Kiem tra trang thai:
docker-compose ps
echo.
echo He thong da khoi dong!
echo.
echo Truy cap:
echo - Frontend: http://localhost:3000
echo - API Gateway: http://localhost:5000
echo - RabbitMQ Management: http://localhost:15672 (guest/guest)
echo.
pause
goto menu

:stop
echo.
echo Dung he thong...
docker-compose down
echo.
echo He thong da dung!
pause
goto menu

:logs
echo.
echo Chon service de xem logs:
echo 1. Tat ca
echo 2. API Gateway
echo 3. User Service
echo 4. Shortener Service
echo 5. Frontend
echo.
set /p logchoice="Nhap lua chon (1-5): "

if "%logchoice%"=="1" docker-compose logs -f
if "%logchoice%"=="2" docker-compose logs -f api-gateway
if "%logchoice%"=="3" docker-compose logs -f user-service
if "%logchoice%"=="4" docker-compose logs -f shortener-service
if "%logchoice%"=="5" docker-compose logs -f frontend

goto menu

:status
echo.
echo Trang thai cac services:
docker-compose ps
echo.
echo Health checks:
echo.
echo API Gateway:
curl -s http://localhost:5000/health
echo.
echo.
echo Shortener Service:
curl -s http://localhost:5000/api/health
echo.
echo.
echo User Service:
curl -s http://localhost:5000/api/auth/health
echo.
echo.
pause
goto menu

:reset
echo.
echo CANH BAO: Thao tac nay se xoa toan bo database!
set /p confirm="Ban co chac chan muon tiep tuc? (Y/N): "
if /i "%confirm%"=="Y" (
    echo.
    echo Dang reset he thong...
    docker-compose down -v
    echo.
    echo Khoi dong lai...
    docker-compose up -d
    echo.
    echo Reset hoan tat!
) else (
    echo.
    echo Da huy thao tac.
)
pause
goto menu

:end
echo.
echo Tam biet!
exit
