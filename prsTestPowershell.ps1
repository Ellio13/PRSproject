Clear-Host
$currentForegroundColor = $host.ui.RawUI.ForegroundColor
$currentBackgroundColor = $host.ui.RawUI.BackgroundColor

$apiurl = "http://localhost:8080"
if ($args.Count -gt 0)
{
  $apiurl = $args[0]
}

$msg = @"
PRStest.ps1 version 1.03

This PowerShell script tests the PRS REST API.
It tests for:
 - Plural vs. singular URLs. (/user vs. /users)
 - The creation of:
   -  1 user
   -  1 vendor
   -  2 products
   -  1 request
   -  2 lineitems
- Setting a request as Approved.
- The Totals feature.
- The user logon feature.
- Changing a property in all five entities.
- Deleting of a user.

Press enter to continue with the URL of $apiurl. 

Press Ctrl-C and relaunch with the URL of your choice:
  .\PRStest.ps1 http://localhost:5257
  .\PRStest.ps1 https://myprs.azurewebsites.net
"@
Write-Host $msg
$temp = Read-Host

"Note:"
Write-Host "  Green is good!" -BackgroundColor green -ForegroundColor White
Write-Host "  White is informational!" 
Write-Host "  Red is bad!" -ForegroundColor Red

Write-Host "--------------------------------------`n`n"

function ShowHeading {
  param ( $txt )
  Write-Host "$txt" -BackgroundColor Green -ForegroundColor White -NoNewline
  Write-Host -BackgroundColor $currentBackgroundColor -ForegroundColor $currentForegroundColor
}
function ShowError {
  param ( $txt )
  Write-Host "  $txt" -ForegroundColor Red -NoNewline
  Write-Host -BackgroundColor $currentBackgroundColor -ForegroundColor $currentForegroundColor
}
function ShowMsg {
  param ( $txt )
  Write-Host "  $txt" -NoNewline
  Write-Host -BackgroundColor $currentBackgroundColor -ForegroundColor $currentForegroundColor
}
function ShowJson {
  param ( $txt )
  Write-Host $txt -NoNewline
  Write-Host "" 
}
function ShowEnd {
  Write-Host "StatusCode:" $_.Exception.Response.StatusCode.value__ -ForegroundColor Black -BackgroundColor White
  #Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription -ForegroundColor Red
  Write-Host "Exception message:" $_.Exception.Message -ForegroundColor Red
  Write-Host "Could not complete tests." -ForegroundColor Red
  Exit
}

ShowHeading "Testing using URL: $apiurl"
Write-Host



try {
$url = $apiurl + "/api/users"
$response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json" -Body $bodyJson
}
catch {
  ShowError "   Could not connect to $url. Testing to see if you used singular URLs. I.e. /user..."
  try {
    $url = $apiurl + "/api/user"
    $response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json" -Body $bodyJson
    }
    catch {
      ShowError "Could not connect to $url. Ending script..."
      ShowEnd    
    }
    ShowError "You URLs are singular and need to be plual. I.e. /users not /user."
    ShowEnd
}

# Variables for objects created from the API
$UserId = 0;
$UserUsername = "";
$UserPassword = "";
$VendorId = 0;
$ProductId1 = 0;
$ProductId2 = 0;
$RequestId = 0;
$RequestLine1Id = 0;
$RequestLine2Id = 0;
$response = $null;


#----------------------------------

ShowHeading "Creating User"
$newid = (Get-Random -Maximum 1000)
$bodyJson = @"
{
    "username": "Sam$newid",
    "password": "topsecret",
    "firstname": "Sam",
    "reviewer": true,
    "lastname": "Jones",
    "phoneNumber" : "123-123-1234",
    "email" : "Sam$newid@example.com",
    "reviewer" : false,
    "admin": true
  }
"@
# phone and email are not optional! (per Sean)

$url = $apiurl + "/api/users"
try {
$response = Invoke-RestMethod $url -Method "POST" -ContentType "application/json" -Body $bodyJson
}
catch
{
  ShowError "Could not create a new user using: "
  ShowJson $bodyJson

  ShowEnd    
}
$UserId = $response.id
$UserUsername = $response.username
$UserPassword = $response.password
ShowMsg "User id $UserId created."



#----------------------------------

ShowHeading "Verifying login for user $UserId."

#$url = $apiurl + "/api/users/" + $UserUsername + "/" + $UserPassword
$url = $apiurl + "/api/users/login"

$bodyJson = @"
{
    "username": "Sam$newid",
    "password": "topsecret"
  }
"@
try {
  $response = Invoke-RestMethod $url -Method "POST" -ContentType "application/json" -Body $bodyJson
  ShowMsg "Logged in successfully"
  ShowMsg ""
  }
catch
{
  ShowError "Could not log in user. "
  ShowEnd
}


#----------------------------------

ShowHeading "Creating Vendor"

$newid = (Get-Random -Maximum 1000)
$bodyJson = @"
{
    "name": "Vendor $newid",
    "address": "South St",
    "city": "Erie",
    "state": "IN",
    "zip": "46000",
    "code": "ven$newid"
}  
"@

$url = $apiurl + "/api/vendors"
try {
  $response = Invoke-RestMethod $url -Method "POST" -ContentType "application/json" -Body $bodyJson
}
catch
{
  ShowError "Could not create a new vendor using: "
  ShowJson $bodyJson
  ShowEnd
}

$VendorId = $response.id
ShowMsg "Vendor id $VendorId created."

#----------------------------------

ShowHeading "Creating Product #1"

$newid = (Get-Random -Maximum 1000)
$prod1price = (Get-Random -Minimum 100 -Maximum 2000)
$bodyJson = @"
{
    "name": "Part A$newid",
    "unit": "ea",
    "partNumber": "A$newid",
    "vendorId": $VendorId,
    "price": $prod1price
} 
"@
# photopath is optional!

$url = $apiurl + "/api/products"
try {
  $response = Invoke-RestMethod $url -Method "POST" -ContentType "application/json" -Body $bodyJson
}
catch
{
  ShowError "Could not create a new product using: "
  ShowJson $bodyJson
  ShowEnd
}


$ProductId1 = $response.id
ShowMsg "Product id $ProductId1 created."


#----------------------------------

ShowHeading "Creating Product #2"

$newid = (Get-Random -Maximum 1000)
$prod2price = (Get-Random -Minimum 100 -Maximum 2000)
$bodyJson = @"
{
    "name": "Part B$newid",
    "unit": "ea",
    "partNumber": "B$newid",
    "vendorId": $VendorId,
    "price": $prod2price
  } 
"@

$url = $apiurl + "/api/products"
try {
  $response = Invoke-RestMethod $url -Method "POST" -ContentType "application/json" -Body $bodyJson
}
catch
{
  ShowError "Could not create a new product at $url using: "
  ShowJson $bodyJson
  ShowEnd
}

$ProductId2 = $response.id
ShowMsg "Product id $ProductId2 created."


#----------------------------------

ShowHeading "Creating Request"

$newid = (Get-Random -Maximum 1000)
$bodyJson = @"
{
    "description": "Request Test $newid",
    "status": "NEW",
    "justification": "Needed for my job.",
    "userId": $UserId,
    "dateNeeded": "2024-06-01T00:00:00",
    "deliveryMode" : "Pickup"
}
"@

$url = $apiurl + "/api/requests"
try {
  $response = Invoke-RestMethod $url -Method "POST" -ContentType "application/json" -Body $bodyJson
}
catch
{
  ShowError "Could not create a new request at $url using: "
  ShowJson $bodyJson
  ShowEnd
}

$RequestId = $response.id
ShowMsg "Request id $RequestId created."
if ($response.DeliveryMode -cne "Pickup") {
    ShowError "DeliveryMode not defaulted to Pickup" -ForegroundColor Red
}
if ($response.Status -cne "NEW") {
    ShowError "DeliveryMode not defaulted to Pickup" -ForegroundColor Red
}



#----------------------------------

ShowHeading "Creating Request Line #1"

$bodyJson = @"
{
    "requestId": $RequestId,
    "quantity": 10,
    "productId": $ProductId1
  }
"@

$url = $apiurl + "/api/lineitems"
try {
  $response = Invoke-RestMethod $url -Method "POST" -ContentType "application/json" -Body $bodyJson
}
catch
{
  ShowError "Could not create a new requestline at $url using: "
  ShowJson $bodyJson
  ShowEnd
}

$RequestLine1Id = $response.id
ShowMsg "RequestLine #1 id $RequestLine1Id created."

#----------------------------------

ShowHeading "Creating Request Line #2"

$bodyJson = @"
{
    "requestId": $RequestId,
    "quantity": 10,
    "productId": $ProductId2
  }
"@

$url = $apiurl + "/api/lineitems"
try {
  $response = Invoke-RestMethod $url -Method "POST" -ContentType "application/json" -Body $bodyJson
}
catch
{
  ShowError "Could not create a new requestline at $url using: "
  ShowJson $bodyJson
  ShowEnd
}

$RequestLine2Id = $response.id
ShowMsg "RequestLine #2 id $RequestLine2Id created."

#----------------------------------

ShowHeading "Getting Request Total"

$url = $apiurl + "/api/requests/" + $RequestId
try {
  $response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json"
}
catch
{
  ShowError "Could not find the request at $url. "
  ShowEnd
}

$total = $response.Total

ShowMsg ("Total should be $prod1price * 10 + $prod2price * 10, or " + ($prod1price * 10 + $prod2price * 10))
ShowMsg "Total is $total"
if ($total -ne ($prod1price * 10 + $prod2price * 10))
{
    ShowError "Request does not have correct total."
    ShowEnd
}

#----------------------------------

ShowHeading "Approving the Request"

$url = $apiurl + "/api/requests/approve/" + $RequestId
try {
  $response = Invoke-RestMethod $url -Method "PUT" -ContentType "application/json"
}
catch
{
  ShowError "Could not approve request at $url. "
}

$url = $apiurl + "/api/requests/" + $RequestId 
$response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json"

if ($response.Status -ceq "APPROVED")
{
  ShowMsg "Approved!"
} else {
  ShowError "Approval was not set!"
  ShowEnd
}

#----------------------------------

ShowHeading "Setting the Request for REVIEW"

$url = $apiurl + "/api/requests/submit-review/" + $RequestId 
try {
  $response = Invoke-RestMethod $url -Method "PUT" # -ContentType "application/json"
}
catch
{
  ShowError "Could not set request for review at $url. "
}

$url = $apiurl + "/api/requests/" + $RequestId 
$response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json"
$sts = $response.Status
if ($sts -ceq "REVIEW" -or $sts -ceq "APPROVED" )
{
  ShowMsg "Set to $sts!"
} else {
  ShowError "REVIEW was not set!"
  ShowEnd
}



#----------------------------------

ShowHeading "Getting Requests for REVIEW using user $UserId."

$url = $apiurl + "/api/requests/list-review/" + $UserId
try {
  $response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json"
  ShowMsg "Requests found:"
  $response | select id,userid,description, {$_.user.id},  {$_.user.firstname}, {$_.user.lastname} | ft # ConvertTo-Json
  $cnt1 = $response.Count
  ShowMsg "Found $cnt1"
  ShowMsg ""
  }
catch
{
  ShowError "Could not retrieve requests for review at $url. "
}

#----------------------------------

ShowHeading "Getting Requests for REVIEW using user other than user $UserId."

$url = $apiurl + "/api/requests/list-review/" + ($UserId+1)
try {
  $response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json"
  ShowMsg "Requests found:"
  $response | select id,userid,description, {$_.user.id},  {$_.user.firstname}, {$_.user.lastname} | ft # ConvertTo-Json
  $cnt2 = $response.Count
  ShowMsg "Found $cnt2 (counts should be different)"
  ShowMsg ""
  }
catch
{
  ShowError "Could not retrieve requests for review at $url. "
}

if ($cnt1 -eq $cnt2)
{
  ShowError "Counts for user $UserId ($cnt1) should not be the same as counts for user $($UserID+1) ($cnt2)."
}



#----------------------------------


# ShowHeading "Attempting to delete user $UserId."

# $url = $apiurl + "/api/users/" + $UserId
# try {
#   $response = Invoke-RestMethod $url -Method "DELETE" -ContentType "application/json"
#   ShowError "Delete worked! (Did you enable cascading deletes or not not setup table relationships?"
#   ShowMsg ""
#   }
# catch
# {
#   ShowMsg "Delete was not permitted, as expected. (User has requests.) "
#   $statuscode = $_.Exception.Response.StatusCode.value__
#   #Write-Host "StatusCode: $statuscode" -ForegroundColor Black -BackgroundColor White
#   ##Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription -ForegroundColor Red
#   #Write-Host "Exception message:" $_.Exception.Message
#   if ($statuscode -ge 500)
#   {
#     ShowError "Returned status code $statuscode, 500 status codes should not be returned to the user."
#   }
# }

#----------------------------------

ShowHeading "Attempting to delete non-existing user $($UserId+100)."

$url = $apiurl + "/api/users/" + ($UserId+100)
try {
  $response = Invoke-RestMethod $url -Method "DELETE" -ContentType "application/json"
  ShowError "Delete reported that it worked! But, the user does not exist"
  ShowMsg ""
  }
catch
{
  ShowMsg "Delete was not permitted as expected user does not exist "
  $statuscode = $_.Exception.Response.StatusCode.value__
  #Write-Host "StatusCode: $statuscode" -ForegroundColor Black -BackgroundColor White
  ##Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription -ForegroundColor Red
  #Write-Host "Exception message:" $_.Exception.Message
  if ($statuscode -ge 500)
  {
    ShowError "Returned status code $statuscode, 500 status codes should not be returned to the user. This should have returned a 404."
  }
}

#----------------------------------

for ($i = 0; $i -lt 5; $i++) 
{
  switch ($i) {
    0 { $id=$UserId; $resource = "users"; $field="lastname"; $value="Updated Name"}
    1 { $id=$VendorId; $resource = "vendors"; $field="name";  $value="Updated Name"}
    2 { $id=$ProductId1; $resource = "products"; $field="name";  $value="Updated Name"}
    3 { $id=$RequestId; $resource = "requests"; $field="justification";  $value="Updated Justification"}
    4 { $id=$Requestline1Id; $resource = "lineitems"; $field="quantity";  $value=123}
  }  
 
  $url = $apiurl + "/api/$resource/$id"  
  ShowHeading "Attempting to update $resource by changing $field at $url"

  try {
    $response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json"
    $response.($field) = $value
    $bodyJson = $response | ConvertTo-Json
    $response = Invoke-RestMethod $url -Method "PUT" -ContentType "application/json" -Body $bodyJson

    $response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json"
    if ($response.($field) -eq $value)
    {
      ShowMsg "Update successfull."
    }
    else
    {
      ShowError "$field was not updated!"
    }
  }
  catch
  {
    ShowError "Update was not successfull."
    $statuscode = $_.Exception.Response.StatusCode.value__
    Write-Host "StatusCode: $statuscode" -ForegroundColor Black -BackgroundColor White
    #Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription -ForegroundColor Red
    Write-Host "Exception message:" $_.Exception.Message
  }
}


#----------------------------------

$resources =  "user","vendor","product","request","requestline"

foreach ($resource in $resources) 
{
  ShowHeading "Attempting to find $resource that does not exist (2000)."

  $url = $apiurl + "/api/$resource/2000"  
  try {
    $response = Invoke-RestMethod $url -Method "GET" -ContentType "application/json"
    ShowError "Reported that it worked! But, does not exist!"
    ShowMsg ""
    }
  catch
  {
    $statuscode = $_.Exception.Response.StatusCode.value__
    ShowMsg "StatusCode: $statuscode (as expected!)"
    #Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription -ForegroundColor Red
    #Write-Host "Exception message:" $_.Exception.Message
    if ($statuscode -ge 500)
    {
      ShowError "Returned status code $statuscode, 500 status codes should not be returned to the user. This should have returned a 404."
    }
  }
}

#----------------------------------







