import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import * as moment from 'moment';

@Component({
  selector: 'app-print-log',
  templateUrl: './print-log.component.html',
  styleUrls: ['../preferences.component.scss', './print-log.component.css']
})
export class PrintLogComponent implements OnInit {
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
  statuses = [
    "Success",
    "Error",
    "InProgress"
  ];

  TotalCount = 0;
  loading: boolean = false;

  Criteria ={
    TextSearch:null,
    Id:null,
    printJobStatus:null,
    UserId:null,
    PrinterId:null,
    Page: 1,
    Limit: 10,
    SortBy: 'Created',
    Ascending :false,
    FromDate: null,
    ToDate: null
  };

  logs = [];
  selectedUser = {};
  selectedPrinter = {};
  selectedStatus = '';

  selectedDate = {startDate: null, endDate: null};

  constructor(private api: ApiService) { }

  ngOnInit() {
    this.getPrintLogs();
    this.getPrinters();
    this.getUsers();
  }

  getPrinters() {
    this.api.GetPrinters({},
      data => {
        this.printers = data.List;
      },
      error => console.log(error)
    );
  }

  getUsers() {
    this.api.GetUsers({Status: true},
      data => {
        this.users = data;
      },
      error => console.log(error)
    );
  }

  getPrintLogs() {
    this.loading = true;
    this.api.GetPrintLogs(
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

  SortBy(sort){

    if(sort == this.Criteria.SortBy) {
      this.Criteria.Ascending = !this.Criteria.Ascending;

    } else {
      this.Criteria.Ascending = true;
    }

    this.Criteria.SortBy = sort;
    this.getPrintLogs();
  }

  selectUser(user) {
    this.selectedUser = user;
    this.Criteria.UserId = user.UserId
    this.getPrintLogs();
  }

  selectPrinter(printer) {
    this.selectedPrinter = printer;
    this.Criteria.PrinterId = printer.PrinterId;
    this.getPrintLogs();
  }

  selectStatus(status) {
    this.selectedStatus = status;
    this.Criteria.printJobStatus = status;
    this.getPrintLogs();
  }

  selectDate() {
    this.Criteria.FromDate = moment(this.selectedDate.startDate).format();
    this.Criteria.ToDate = moment(this.selectedDate.endDate).format();
    this.getPrintLogs();
  }

  setPcsOnpage(pcs) {
    this.Criteria.Limit = pcs;
    this.getPrintLogs();
  }

  paginate(event: any) {
    this.Criteria.Page = event.page;
    this.Criteria.Limit = event.itemsPerPage;
    this.getPrintLogs();
  }

  exportPrintLog() {
    this.api.exportPrintLog(this.Criteria);
  }

}
