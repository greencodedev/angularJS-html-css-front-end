import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { Observable, observable, of } from 'rxjs';
import { mergeMap } from 'rxjs/operators';
import { TypeaheadMatch } from 'ngx-bootstrap';
import * as moment from 'moment';

@Component({
  selector: 'app-convert-log',
  templateUrl: './convert-log.component.html',
  styleUrls: ['./convert-log.component.css']
})
export class ConvertLogComponent implements OnInit {

  pcsVariables = [
    5,
    10,
    20
  ];

  smallnumPages = 0;
  itemsPerPage = 10;
  currentPage = 1;
  printers: any[];
  users: any[];

  
  TotalCount = 0;
  loading: boolean = false;
  Criteria = {
    TextSearch: null,
    Id: null,
    printJobStatus: null,
    UserId: null,
    PrinterId: null,
    Page: 1,
    Limit: 10,
    SortBy: 'Created',
    Ascending: false,
    FromDate: null,
    ToDate: null,
    ProductId: null,
    SubLocationId: null,
  };
  logs = [];
  selectedUser = {};
  selectedPrinter = {};
  selectedStatus = '';
  selectedDate = {startDate: null, endDate: null};

  productSelected: string;
  productsDataSource: Observable<any>;
  sublocationSelecte: string;
  locationDateSource: Observable<any>;

  constructor(private api: ApiService) {
  }

  ngOnInit() {
    this.getConvertLogs();
    this.getUsers();

    this.productsDataSource = Observable.create((observer: any) => {
      observer.next(this.productSelected);
    })
    .pipe(mergeMap((token: string) => this.api.FilterProducts(token)));

    this.locationDateSource = Observable.create((observer: any) => {
      observer.next(this.sublocationSelecte);
    })
    .pipe(mergeMap((token: string) => this.api.FilterSublocations(token)));
  }

  selectProduct(e: TypeaheadMatch): void {
    this.Criteria.ProductId = e.item.ProductId;
    this.Criteria.Page = 1;
    this.getConvertLogs();
  }
  productChanged() {
    if(this.productSelected == '') {
      this.Criteria.ProductId = null;
      this.Criteria.Page = 1;
      this.getConvertLogs();
    }
  }

  selectSubLocation(e: TypeaheadMatch): void {
    this.Criteria.SubLocationId = e.item.SublocationId;
    this.Criteria.Page = 1;
    this.getConvertLogs();
  }
  subLocationChanged() {
    if(this.sublocationSelecte == '') {
      this.Criteria.SubLocationId = null;
      this.Criteria.Page = 1;
      this.getConvertLogs();
    }
  }

  

  getUsers() {
    this.api.GetUsers({Status: true},
      data => {
        this.users = data;
      },
      error => console.log(error)
    );
  }

  getConvertLogs() {
    let $this = this;
    this.loading = true;
    this.api.GetConvertLogs(
      this.Criteria,
      data => {
        this.logs = data.List;
        this.TotalCount = data.TotalCount;
        this.loading = false;
      },
      error => {
        this.loading = false;
        alert(error.Messages.join(','));
      }
    );
  }

  SortBy(sort) {

    if (sort == this.Criteria.SortBy) {
      this.Criteria.Ascending = !this.Criteria.Ascending;

    } else {
      this.Criteria.Ascending = true;
    }

    this.Criteria.SortBy = sort;
    this.Criteria.Page = 1;
    this.getConvertLogs();
  }

  selectUser(user) {
    this.selectedUser = user;
    this.Criteria.UserId = user.UserId
    this.Criteria.Page = 1;
    this.getConvertLogs();
  }

  selectDate() {
    this.Criteria.FromDate = moment(this.selectedDate.startDate).format();
    this.Criteria.ToDate = moment(this.selectedDate.endDate).format();
    this.Criteria.Page = 1;
    this.getConvertLogs();
  }

  setPcsOnpage(pcs) {
    this.Criteria.Limit = pcs;
  }


  paginate(event: any) {
    this.Criteria.Page = event.page;
    this.Criteria.Limit = event.itemsPerPage;
    this.getConvertLogs();
  }

  exportConvertLog() {
    this.api.exportConversionLog(this.Criteria);
  }

}
