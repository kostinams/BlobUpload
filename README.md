# BlobUpload
Sample to showcase a reliable way of uploading blob files into Azure Storage accounts.
This sample is an addition to article about FrontDoor and Azure Storage accounts configuration https://techcommunity.microsoft.com/t5/fasttrack-for-azure/mission-critical-content-upload-using-azure-frontdoor-and-azure/ba-p/3962207 

GetContainerLocation.cs contains Azure Function with takes parameter 'blobname' and generates upload URLs with SAS for multiple storage accounts distributed in multiple Azure regions (in this example storage accounts are 01uksouth, 02uksouth, 01ukwest, 02ukwest) 
