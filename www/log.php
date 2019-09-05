<?php

$parId = $_POST["sid"];
$seqNum = $_POST["seq"];


$planPath = __DIR__ . "/sessions/{$parId}";

if (!file_exists($parId)) {
    mkdir($parId, 0777, true);
}

//Define file locations
$domainFile = "{$planPath}/domain{$seqNum}mm.pddl";
$problemFile = "{$planPath}/problem{$seqNum}mm.pddl";
$chronologyFile = "{$planPath}/chronology{$seqNum}mm.pddl";

//Upload Files
foreach($_FILES as $index=>$data) {
    if($data['error'] !== UPLOAD_ERR_OK) {
        echo "ERROR";
        exit();
    }
    switch($index) {
        case "domain":
            move_uploaded_file($data['tmp_name'], $domainFile);
            break;
        
        case "problem":
            move_uploaded_file($data['tmp_name'], $problemFile);
            break;
        
        case "chronology":
            move_uploaded_file($data['tmp_name'], $chronologyFile);
            break;

    }
}


$logFile = "{$planPath}/log.txt";

$logEntry = $_POST["log"];

if(false === file_put_contents($logFile, "{$seqNum}-{$logEntry}".PHP_EOL , FILE_APPEND | LOCK_EX)) {
    echo "ERROR";
    exit();
}


//Send output
header('Expires: 0');
header('Cache-Control: must-revalidate');
echo "OK";
