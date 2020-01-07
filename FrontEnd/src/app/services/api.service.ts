import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { CookieService } from 'ngx-cookie';
import { Router } from '@angular/router';
import { saveAs } from 'file-saver';
import { Observable, Subscription } from 'rxjs';
import { map, tap } from 'rxjs/operators';



@Injectable({
  providedIn: 'root'
})
export class ApiService {
  constructor(
    private http: HttpClient,
    private cookie: CookieService,
    private router: Router
    ) { }

    // product functions
  SearchProducts(Criteria, ChannelId, success, error) {
 
    var option = {
      method: 'POST',
      url: 'api/products/' + ChannelId,
      data: Criteria
    };

    this.makeApiRequest(option, success, error);
  }

  FilterProducts(searchTem) {

    var url = environment.apiBaseUrl + 'api/products';
    var httpOptions = this.getHttpOptions(false);

    return this.http.post(url, {TextSearch:searchTem }, httpOptions)
    .pipe(map(response => response['Data']['List']));
  }

  // password functions
  ForgetPassword(email, success, error) {
 
    var option = {
      method: 'POST',
      url: 'forgatPassword',
      data: email
    };

    this.makeApiRequest(option, success, error);
  }

  resetPassword(data, success, error) {
 
    var option = {
      method: 'POST',
      url: 'ResetPassword',
      data: data
    };

    this.makeApiRequest(option, success, error);
  }


  // categories functions
  GetCategories(success, error) {
 
    var option = {
      method: 'GET',
      url: 'api/category'
    };

    this.makeApiRequest(option, success, error);
  }

  SearchCategory(success, error) {

    var option = {
      method: 'GET',
      url: 'api/category',
      data: {}
    };

    this.makeApiRequest(option, success, error);
  }

  // users functions
  GetUsers(search, success, error) {
 
    var option = {
      method: 'POST',
      url: 'Users',
      data: search
    };

    this.makeApiRequest(option, success, error);
  }

  AddUser(user, success, error) { 
 
    var option = {
      method: 'POST',
      url: 'add',
      data: user
    };

    this.makeApiRequest(option, success, error);
  }

  UpdateUser(user, success, error) { 
 
    var option = {
      method: 'POST',
      url: 'UpdateUsers',
      data: user
    };

    this.makeApiRequest(option, success, error);
  }

  GetCurrentUsers(success, error) {
 
    var option = {
      method: 'GET',
      url: 'CurrentUser'
    };

    this.makeApiRequest(option, success, error);
  }

  GetCurrentAccount(success, error) {
 
    var option = {
      method: 'GET',
      url: 'api/Account'
    };

    this.makeApiRequest(option, success, error);
  }



  GetConditions(data, success, error) {
 
    var option = {
      method: 'POST',
      url: 'api/conditions',
      data: data
    };

    this.makeApiRequest(option, success, error);
  }

  GetProductGroups(success, error) {
    var option = {
      method: 'GET',
      url: 'api/products/groups'
    };

    this.makeApiRequest(option, success, error);
  }

  UpdateConditions(data, success, error) {
    var option = {
      method: 'POST',
      url: 'api/conditions/update',
      data: data
    };

    this.makeApiRequest(option, success, error);
  }

  UpdateUserConditions(UserId, Conditions, success, error) {
    var option = {
      method: 'POST',
      url: 'api/conditions/user/' + UserId,
      data: Conditions
    };

    this.makeApiRequest(option, success, error);
  }

  GetSublocations(search, ChannelID, success, error) {
    var option = {
      method: 'POST',
      url: 'api/sublocations/' + ChannelID,
      data: search
    };

    this.makeApiRequest(option, success, error);
  }

  UpdateUserLocations(UserId, Sublocations, ChannelId, success, error) {
    var option = {
      method: 'POST',
      url: 'api/sublocations/user/' + UserId + "/" + ChannelId,
      data: Sublocations
    };

    this.makeApiRequest(option, success, error);
  }

  FilterSublocations(searchTem) {

    var url = environment.apiBaseUrl + 'api/sublocations';
    var httpOptions = this.getHttpOptions(false);

    return this.http.post(url, {TextSearch:searchTem }, httpOptions)
    .pipe(map(response => response['Data']));
  }

  UpdateSublocations(data, ChannelId,  success, error) {
    var option = {
      method: 'POST',
      url: 'api/sublocations/update/' + ChannelId,
      data: data
    };

    this.makeApiRequest(option, success, error);
  }

  ConvertStock(conversion, ChannelId, success, error) {
    var option = {
      method: 'POST',
      url: 'api/convert/' + ChannelId,
      data: conversion
    };

    this.makeApiRequest(option, success, error);
  }

  GetConvertLogs(search, success, error) {
    var option = {
      method: 'POST',
      url: 'api/convert/logs',
      data: search
    };

    this.makeApiRequest(option, success, error);
  }

  GetAvgCostLogs(search, success, error) {
    var option = {
      method: 'POST',
      url: 'api/avgcost/logs',
      data: search
    };

    this.makeApiRequest(option, success, error);
  }


// sync
  Sync(ChannelId, success, error){
    var option = {
      method: 'GET',
      url: 'api/sync/' + ChannelId,
      data: {}
    };

    this.makeApiRequest(option, success, error);
  }

  GetLastSync(type, success, error) {
    var option = {
      method: 'GET',
      url: 'api/sync/lastSynced/' + type
    };
    
    this.makeApiRequest(option, success, error);
  }

  //login

  login(userName, password, success, errorCallback) {
    var $this = this;

    this.tokenRequest(
      userName,
      password,
      function(data) {
        
        var now = new Date();
        now.setDate(now.getDate() + 30);
        $this.cookie.put('EMBApiToken', data.access_token, {expires: now});
        success();
      },
      function(error) {
        errorCallback(error);
      }
    );
  }

  LogOut() {
    this.cookie.remove('EMBApiToken');
    this.router.navigate(['sign-in']);
  }


  private tokenRequest(userName, password, success, errorCallback) {
    var url = environment.apiBaseUrl + 'Token';

    var body = new HttpParams()
      .set('grant_type', 'password')
      .set('username', userName)
      .set('password', password);

    var httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/x-www-form-urlencoded'
      })
    };

    this.http
      .post(url, body.toString(), httpOptions)
      .subscribe(data => success(data), error => errorCallback(error.error));
  }


  // PRINTING
  GetPrinters(Criteria, success, error) {
    var option = {
      method: 'POST',
      url: 'api/print/GetPrinters',
      data: Criteria
    };
    this.makeApiRequest(option, success, error);
  }

  UpdatePrinter(data, success, error) {
    var option = {
      method: 'POST',
      url: 'api/print/printers',
      data: data
    };
    this.makeApiRequest(option, success, error);
  }

  GetPrinterSizes(success, error) {
    var option = {
      method: 'GET',
      url: 'api/print/printersizes'
    };
    this.makeApiRequest(option, success, error);
  }

  UpdatePrinterSize(data, success, error) {
    var option = {
      method: 'POST',
      url: 'api/print/printersizes',
      data: data
    };
    this.makeApiRequest(option, success, error);
  }
syncPrinters(success, error){
  var option = {
    method: 'GET',
    url: 'api/print/printers/sync'
  };
  this.makeApiRequest(option, success, error);
}
  CreatePrintJob(data, success, error) {
    var option = {
      method: 'POST',
      url: 'api/print/printjobs',
      data: data
    };
    this.makeApiRequest(option, success, error);
  }

  GetPrintJob(printJobId, success, error) {
    var option = {
      method: 'GET',
      url: 'api/print/printjobs/' + printJobId,
      data: {}
    };
    this.makeApiRequest(option, success, error);
  }

  GetPrintLogs(search, success, error) {
    var option = {
      method: 'POST',
      url: 'api/print/printjoblogs',
      data: search
    };

    this.makeApiRequest(option, success, error);
  }

  exportPrintLog(search) {
    this.downloadFile('api/print/printjoblogs/export', search, 'EMB-PrintLog-Export.csv');
  }

  exportConversionLog(search) {
    this.downloadFile('api/convert/logs/export', search, 'EMB-ConversionLog-Export.csv');
  }

  exportAvgCostLog(search) {
    this.downloadFile('api/avgcost/logs/export', search, 'EMB-AvgCostLog-Export.csv');
  }

  private downloadFile(url, params, fileName) {

    Object.keys(params).forEach((key) => (params[key] == null) && delete params[key]);

    var httpOptions: any = {
      headers: new HttpHeaders({
        'Authorization': 'Bearer ' + this.cookie.get('EMBApiToken')
      }),
      responseType: "arraybuffer",
      observe: 'response',
      params: params
    };

    this.http.get(environment.apiBaseUrl + url, httpOptions).subscribe((response: any) => {
      var data = response.body;
      const headers = response.headers;
      var type = headers.get('Content-Type');
      var blob = new Blob([data], { type: type });
      saveAs(blob, fileName);
    }, error => {
      console.log(error);
    });
  }

  private makeApiRequest(option, success, errorCallback): Subscription {
    var $this = this;

    return this.apiRequest(option, success, function(error) {

      //console.log(error.Messages.join(', '));
      //alert(error.Messages.join(', '));

      if (
        error.Message &&
        error.Message == 'Authorization has been denied for this request.'
      ) {

        $this.router.navigate(['/sign-in']);
      } else{

        errorCallback(error);

      }
    });
  }

  private apiRequest(option, success, errorCallback): Subscription {

    var url = environment.apiBaseUrl + option.url;
    var httpOptions = this.getHttpOptions(option.isFile);

    if (option.method == 'GET') {
      return this.http
        .get(url, httpOptions)
        .subscribe(
          data => success(data['Data']),
          error => errorCallback(error.error)
        );
    } else if (option.method == 'POST') {
      return this.http
        .post(url, option.data, httpOptions)
        .subscribe(
          data => success(data['Data']),
          error => errorCallback(error.error)
        );
    } else if (option.method == 'PUT') {
      return this.http
        .put(url, option.data, httpOptions)
        .subscribe(
          data => success(data['Data']),
          error => errorCallback(error.error)
        );
    } else if (option.method == 'DELETE') {
      return this.http
        .delete(url, httpOptions)
        .subscribe(
          data => success(data['Data']),
          error => errorCallback(error.error)
        );
    }
  }

  private getHttpOptions(isFile: boolean = false) {

    var headers = {
      Authorization: 'Bearer ' + this.cookie.get('EMBApiToken')
    };

    if(!isFile) {
      headers['Content-Type'] = 'application/json';
    }

    var httpOptions: any = {
      headers: new HttpHeaders(headers)
    };

    return httpOptions;
  }
}
