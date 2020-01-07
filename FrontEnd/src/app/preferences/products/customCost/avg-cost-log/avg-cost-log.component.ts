import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { Observable, observable, of } from 'rxjs';
import { mergeMap } from 'rxjs/operators';
import { TypeaheadMatch } from 'ngx-bootstrap';
import * as moment from 'moment';

@Component({
  selector: 'app-avg-cost-log',
  templateUrl: './avg-cost-log.component.html',
  styleUrls: ['./avg-cost-log.component.css']
})
export class AvgCostLogComponent implements OnInit {

  pcsVariables = [
    5,
    10,
    20
  ];

  smallnumPages = 0;
  itemsPerPage = 10;
  currentPage = 1;
  
  TotalCount = 0;
  loading: boolean = false;
  Criteria = {
    TextSearch: null,
    Page: 1,
    Limit: 10,
    SortBy: 'ReceivedDate',
    Ascending: false,
    FromDate: null,
    ToDate: null,
    ProductId: null,
  };
  logs = [];
  selectedDate = {startDate: null, endDate: null};

  productSelected: string;
  productsDataSource: Observable<any>;
  sublocationSelecte: string;
  locationDateSource: Observable<any>;

  constructor(private api: ApiService) {
  }

  ngOnInit() {
    this.getLogs();

    this.productsDataSource = Observable.create((observer: any) => {
      observer.next(this.productSelected);
    })
    .pipe(mergeMap((token: string) => this.api.FilterProducts(token)));
  }

  selectProduct(e: TypeaheadMatch): void {
    this.Criteria.ProductId = e.item.ProductId;
    this.Criteria.Page = 1;
    this.getLogs();
  }
  productChanged() {
    if(this.productSelected == '') {
      this.Criteria.ProductId = null;
      this.Criteria.Page = 1;
      this.getLogs();
    }
  }

  

  getLogs() {
    let $this = this;
    this.loading = true;
    this.api.GetAvgCostLogs(
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
    this.getLogs();
  }

  selectDate() {
    this.Criteria.FromDate = moment(this.selectedDate.startDate).format();
    this.Criteria.ToDate = moment(this.selectedDate.endDate).format();
    this.Criteria.Page = 1;
    this.getLogs();
  }

  setPcsOnpage(pcs) {
    this.Criteria.Limit = pcs;
    this.Criteria.Page = 1;
    this.getLogs();
  }


  paginate(event: any) {
    this.Criteria.Page = event.page;
    this.Criteria.Limit = event.itemsPerPage;
    this.getLogs();
  }

  exportLog() {
    this.api.exportAvgCostLog(this.Criteria);
  }

}
