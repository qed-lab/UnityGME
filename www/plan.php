<?php

header('Expires: 0');
header('Cache-Control: must-revalidate');
$executable = __DIR__ . "/siw-then-bfsf";

$parId = $_POST["sid"];
$seqNum = addslashes($_POST["seq"]);


$planPath = __DIR__ . "/sessions/{$parId}";
error_reporting(-1);
ini_set('display_errors', 'On');

if(!file_exists($planPath)) {
    mkdir($planPath, 0777, true);
}

//Define file locations
$domainFile = "{$planPath}/domain{$seqNum}.pddl";
$problemFile = "{$planPath}/problem{$seqNum}.pddl";

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

    }
}

//Plan
$outFile = "{$planPath}/plan{$seqNum}.ipc";
$planResult = shell_exec("{$executable} --domain {$domainFile} --problem {$problemFile} --output {$outFile} 2>&1");

//Send output
@readfile($outFile);

