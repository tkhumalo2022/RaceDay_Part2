# QA ONLY. Run exclusively with both apps bound to loopback and the isolated QA SQL database.
# Requires an already running Part 2 API (127.0.0.1:5001) and MVC portal (127.0.0.1:5250).
$ErrorActionPreference = 'Stop'
if ($env:RACEDAY_QA_APPROVED -ne 'YES') { throw 'Set RACEDAY_QA_APPROVED=YES for the isolated QA environment.' }
$portal = 'http://127.0.0.1:5250'
function Page($path, $session) {
    Invoke-WebRequest -Uri ($portal + $path) -WebSession $session -UseBasicParsing -TimeoutSec 30
}
function Token($html) {
    $match = [regex]::Match($html, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (-not $match.Success) { throw 'Missing MVC CSRF token on form' }
    return $match.Groups[1].Value
}
function Send($path, $session, $body) {
    $result = Invoke-WebRequest -Uri ($portal + $path) -Method Post -Body $body -WebSession $session -UseBasicParsing -TimeoutSec 30 -MaximumRedirection 8
    return $result
}
function Ensure($condition, $name) {
    if (-not $condition) { throw "QA FAILED: $name" }
    Write-Output "PASS: $name"
}
function CheckBody($result, $regex, $name) {
    Ensure ($result.Content -match $regex) $name
}
$suffix = [guid]::NewGuid().ToString('N').Substring(0,8)
$org = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$participant = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$pass = 'Qa!' + [guid]::NewGuid().ToString('N') + '8'
$orgRegister = Page '/Account/Register' $org
$o = Send '/Account/Register' $org @{
    __RequestVerificationToken=(Token $orgRegister.Content)
    FirstName='RaceDay';LastName='OrganiserQA';Email="organiser-$suffix@example.test"
    Role='Organiser';OrganiserName='RaceDay QA Club';Password=$pass;ConfirmPassword=$pass
}
Ensure ($o.StatusCode -eq 200) 'Organiser register HTTP 200 after redirect'
CheckBody $o 'Sign out|Your events' 'Organiser authentication established'
$dashboard = Page '/Dashboard/Index' $org
CheckBody $dashboard 'CREATE EVENT' 'Organiser dashboard loads'
$create = Page '/Organiser/Create' $org
CheckBody $create 'Event name' 'Event form available'
$eventName="QA Race $suffix"
$saved = Send '/Organiser/Save' $org @{
    __RequestVerificationToken=(Token $create.Content)
    Id=0;EventName=$eventName;Description='Isolated QA race for Part 3 validation.'
    Location='Durban QA Circuit';EventDate='2026-11-20';ClosingDate='2026-11-18'
    EventType='Run';Status='Open'
}
CheckBody $saved ([regex]::Escape($eventName)) 'Organiser can create event'
$eventId = [regex]::Match($saved.BaseResponse.ResponseUri.AbsolutePath, '/(?:Home/)?Details/(\d+)').Groups[1].Value
Ensure (-not [string]::IsNullOrWhiteSpace($eventId)) 'Created event ID available'
$catForm = Page ("/Organiser/Category?eventId=" + $eventId) $org
$cat = Send '/Organiser/SaveCategory' $org @{
    __RequestVerificationToken=(Token $catForm.Content)
    Id=0;EventId=$eventId;CategoryName='10 km Open';DistanceKm='10';EntryFee='50';MaxParticipants='80'
}
CheckBody $cat '10 km Open' 'Category creation through Part 2 API'
$pRegister = Page '/Account/Register' $participant
$registered=Send '/Account/Register' $participant @{
    __RequestVerificationToken=(Token $pRegister.Content)
    FirstName='RaceDay';LastName='ParticipantQA';Email="participant-$suffix@example.test"
    Role='Participant';DateOfBirth='2002-01-01';Password=$pass;ConfirmPassword=$pass
}
CheckBody $registered 'Sign out|Your enrolments' 'Participant authentication established'
$searched = Page ('/Home/Index?query=' + [uri]::EscapeDataString($eventName)) $participant
CheckBody $searched ([regex]::Escape($eventName)) 'Event-name search finds a published race'
$searchedByPlace = Page '/Home/Index?query=Durban%20QA%20Circuit' $participant
CheckBody $searchedByPlace ([regex]::Escape($eventName)) 'Location search finds the correct event'
$noMatch = Page ('/Home/Index?query=absent-' + $suffix) $participant
Ensure ($noMatch.Content -notmatch [regex]::Escape($eventName)) 'Unmatched search excludes other events'
$detail = Page ("/Home/Details/$eventId") $participant
CheckBody $detail 'ENROL NOW' 'Participant sees eligible category'
$cats = Invoke-RestMethod -Uri ('http://127.0.0.1:5001/api/events/' + $eventId + '/categories') -WebSession $participant -TimeoutSec 10
$categoryId = @($cats | Where-Object CategoryName -eq '10 km Open')[0].id
Ensure ($categoryId -gt 0) 'Real category ID retrieved from API'
$enrol = Send '/Participant/Enrol' $participant @{
    __RequestVerificationToken=(Token $detail.Content)
    eventId=$eventId;categoryId=$categoryId
}
CheckBody $enrol 'Your enrolments' 'Participant enrolment completes'
$prof = Page '/Participant/Profile' $participant
CheckBody $prof 'Your details' 'Participant profile form loads'
$edited = Send '/Participant/Profile' $participant @{
    __RequestVerificationToken=(Token $prof.Content)
    FirstName='UpdatedQA';LastName='ParticipantQA';PhoneNumber='0712345678';DateOfBirth='2002-01-01'
}
CheckBody $edited 'UpdatedQA' 'Participant updates profile through API'
$start = Page ("/Organiser/Entries/$eventId") $org
CheckBody $start 'ParticipantQA' 'Organiser sees enrolment'
$editPage = Page ("/Organiser/Edit/" + $eventId) $org
CheckBody $editPage 'EDIT YOUR EVENT' 'Organiser can edit event'
$updated = Send '/Organiser/Save' $org @{
    __RequestVerificationToken=(Token $editPage.Content)
    Id=$eventId;EventName=$eventName;Description='Completed isolated QA race.'
    Location='Durban QA Circuit';EventDate='2026-10-30';ClosingDate='2026-10-29'
    EventType='Run';Status='Completed'
}
CheckBody $updated ([regex]::Escape($eventName)) 'Event update through API'
$entriesFromApi = Invoke-RestMethod -Uri ('http://127.0.0.1:5001/api/events/' + $eventId + '/enrolments') -WebSession $org -TimeoutSec 10
$entryId = @($entriesFromApi)[0].id
Ensure ($entryId -gt 0) 'Organiser can retrieve enrolment IDs'
$resultPage = Page ("/Organiser/Result?eventId=$eventId&enrolmentId=$entryId") $org
CheckBody $resultPage 'PUBLISH RESULT' 'Result entry form loads'
$published = Send '/Organiser/Record' $org @{
    __RequestVerificationToken=(Token $resultPage.Content)
    EventId=$eventId;EnrolmentId=$entryId;EventName=$eventName
    ParticipantName='UpdatedQA ParticipantQA';ResultStatus='Finished'
    StartTime='08:00:00';FinishTime='08:43:00';OverallPosition=1
}
CheckBody $published 'Race result recorded' 'Organiser publishes finish time and position'
$participantDashboard = Page '/Dashboard/Index' $participant
CheckBody $participantDashboard 'Position: 1' 'Participant sees published finishing position'
CheckBody $participantDashboard '08:43:00' 'Participant sees recorded finish time'
$restrictedStatus=0
try { $restrictedResponse=Page '/Organiser/Create' $participant; $restrictedStatus=[int]$restrictedResponse.StatusCode }
catch [System.Net.WebException] { $restrictedStatus=[int]$_.Exception.Response.StatusCode }
Ensure ($restrictedStatus -eq 403) 'Participant receives HTTP 403 for Organiser-only create screen'
# Image upload smoke runs against local Azurite (never production Storage).
Add-Type -AssemblyName System.Net.Http
function UploadImage($path, $session, $csrf, $eventId = $null) {
    $handler = New-Object System.Net.Http.HttpClientHandler
    $handler.CookieContainer = $session.Cookies
    $handler.AllowAutoRedirect = $false
    $client = New-Object System.Net.Http.HttpClient($handler)
    $form = New-Object System.Net.Http.MultipartFormDataContent
    $form.Add([System.Net.Http.StringContent]::new($csrf), '__RequestVerificationToken')
    if ($null -ne $eventId) { $form.Add([System.Net.Http.StringContent]::new([string]$eventId), 'id') }
    $png = [Convert]::FromBase64String('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9YNwUxwAAAAASUVORK5CYII=')
    $file = [System.Net.Http.ByteArrayContent]::new($png)
    $file.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::new('image/png')
    $form.Add($file, 'file', 'qa-image.png')
    try {
        $response = $client.PostAsync(($portal+$path), $form).GetAwaiter().GetResult()
        $content = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        Ensure ([int]$response.StatusCode -eq 302) ('Image upload redirected after processing: '+[int]$response.StatusCode+' '+$path)
        return $content
    } finally { $client.Dispose(); $handler.Dispose(); $form.Dispose() }
}
$orgEvent = Page ("/Home/Details/$eventId") $org
$deniedEnrolStatus=0
try {
    Send '/Participant/Enrol' $org @{
        __RequestVerificationToken=(Token $orgEvent.Content)
        eventId=$eventId;categoryId=$categoryId
    } | Out-Null
    $deniedEnrolStatus=200
}
catch [System.Net.WebException] { $deniedEnrolStatus=[int]$_.Exception.Response.StatusCode }
Ensure ($deniedEnrolStatus -eq 403) 'Organiser receives HTTP 403 for Participant enrolment action'
Ensure ($orgEvent.Content -match ('name="id"\s+value="' + $eventId + '"')) 'Banner form includes event id hidden input'
$bannerRedirect = UploadImage '/Organiser/Banner' $org (Token $orgEvent.Content) $eventId
$bannerResult = (Page ("/Home/Details/$eventId") $org).Content
Ensure ($bannerResult -match 'Banner saved to private image storage.') 'Organiser banner stored through API in Azurite'
$bannerBytes = Invoke-WebRequest -Uri ($portal+"/Images/Event/$eventId") -WebSession $org -UseBasicParsing -TimeoutSec 15
Ensure ($bannerBytes.StatusCode -eq 200 -and $bannerBytes.Headers['Content-Type'] -match 'image/png') 'Event banner retrieved from private Azure container'
$profileImagePage = Page '/Participant/Profile' $participant
$pictureRedirect = UploadImage '/Participant/Picture' $participant (Token $profileImagePage.Content)
$pictureResult = (Page '/Participant/Profile' $participant).Content
Ensure ($pictureResult -match 'Profile picture saved to private image storage.') 'Participant picture stored through API in Azurite'
$pictureBytes = Invoke-WebRequest -Uri ($portal+'/Images/Profile') -WebSession $participant -UseBasicParsing -TimeoutSec 15
Ensure ($pictureBytes.StatusCode -eq 200 -and $pictureBytes.Headers['Content-Type'] -match 'image/png') 'Profile picture retrieved by authenticated owner'
Write-Output "QA COMPLETE: event=$eventId; suffix=$suffix (isolated QA database and Azurite)"
