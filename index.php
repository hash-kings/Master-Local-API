<?php
error_reporting(E_ALL);
ini_set('display_errors',1);

$id = 'DU6IdS2gVog';
$cache_path = '';
$filename = 'zergapi.json';

if( file_exists($filename) && ( time() - 300 < filemtime($filename) ) )
{
    $data = json_decode(file_get_contents($filename), true);
	//print_r($data);
}
else
{
    $data = file_get_contents('http://api.zergpool.com:8080/api/status');
    file_put_contents($filename, $data);
    $data = json_decode($data, true);
	//print_r($data);
}
echo json_encode($data);
?>

