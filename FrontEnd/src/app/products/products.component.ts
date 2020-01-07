import { Component, OnInit } from '@angular/core';
import { ApiService } from '../services/api.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { BsModalRef } from 'ngx-bootstrap/modal/bs-modal-ref.service';
import { ModalComponent } from '../Components/modal/modal.component';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { ConvertModalComponent } from '../Components/convert-modal/convert-modal.component';



@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css']
})

export class ProductsComponent implements OnInit {
  selectedCategory = {};
  selectedCondition = {};
  selectedProductGroup = "";

  pcsVariables = [
    5,
    10,
    20
  ]

  smallnumPages: number = 0;
  modalRef: BsModalRef;
  returnedArray: string[];
  printers: any[];
  products: any[] = [];
  conditions: any[];
  productGroups: any[];
  categories: any[];
  TotalCount = 0;
  syncing: boolean = false;
  loading: boolean = false;
  lastSynced;

  Criteria = {
    TextSearch:null,
    Conditions:null,
    Ncs: null,
    Name: null,
    Id:null,
    category:null,
    Page: 1,
    Limit: 10,
    SortBy: 'FinaleId',
   Ascending :true
  };
  NameBool:boolean = null;
  ProductIdBool:boolean = null;
  LabeleBool:boolean = false;
  ConditionsBool:boolean = false;
  userHasConditionsToConvertTo: boolean = false;
  userHasLocationsToConvertFrom: boolean = false;
  userFromConditions: any[];
  focus = false;
  Sublocations: any;
  ConversionPoNumber: string;
  User: any= {};


  constructor(private modalService: BsModalService, private api: ApiService) { }

  ngOnInit() {

    this.api.GetCurrentUsers(
      data => {
        this.User = data;


        this.UserHasConditionsToConvertTo();
        this.UserHasLocationsToConvertFrom();
      },
    error => console.log(error)
    );

    this.getProducts();
    this.getCategories();
    this.getConditions();
    this.getProductGroups();
    // this.GetSublocations();
    this.getPrinters();
    this.GetLastSynced();



  }

  openPrintModal(product, fromConvert = false, qty = 1) {
    this.modalRef = this.modalService.show(ModalComponent, {
      initialState: {
        product: product,
        printers: this.printers,
        fromConvert: fromConvert,
        quantity: qty
      }
    });
  }

  openConvertModal(product) {
    let $this = this;
    this.modalRef = this.modalService.show(ConvertModalComponent, {
      initialState: {
        product: product,
        printers: this.printers,
        // conditions: this.conditions,
        sublocations: this.Sublocations
      },
        class: 'modal-content--wide'
      },
    );

    this.modalRef.content.action.subscribe((result) => {
      $this.getProducts();
      if(result.printWhenDone) {
        $this.openPrintModal(result.conversion.ToProduct, true, result.conversion.Quantity + 1);
      }
      
    });
  }

  setPcsOnpage(pcs) {
    this.Criteria.Limit = pcs;
    this.getProducts();
  }
  selectCategory(category) {
    this.selectedCategory = category;
    this.Criteria.category = category.CategoryId
    this.getProducts();
  }
  selectCondition(condition) {
    this.selectedCondition = condition;
    this.Criteria.Conditions = condition.ConditionId;
    this.getProducts();
  }
  selectProductGroup(productGroup) {
    this.selectedProductGroup = productGroup;
    this.Criteria.Ncs = productGroup;
    this.getProducts();
  }

  groupChanged() {
    if(this.selectedProductGroup == '') {
      this.Criteria.Ncs = null;
      this.getProducts();
    }
  }

  // GetSublocations() {
  //   var search = {
  //     OnlySublocation: true
  //   };
  //   var $this = this;

  //   this.api.GetSublocations(
  //     search,
  //     data => {
  //       $this.Sublocations = data;
  //     },
  //     error => {
  //       this.loading = false;
  //       alert(error.Messages.join(','));
  //     }
  //   );
  // }

  paginate(event: any) {
    //console.log('paginate', event);
    this.Criteria.Page = event.page;
    this.Criteria.Limit = event.itemsPerPage;
    this.getProducts();
  }

  getProducts() {
    //console.log(this.Criteria.Page + ' ' + this.Criteria.Limit);
    this.loading = true;
    this.api.SearchProducts(
      this.Criteria, 1, //here goes the channel id
      data => {
        this.products = data.List;
        this.TotalCount = data.TotalCount;
        this.loading = false;
      },
      error => {

        this.loading = false;
        alert(error.Messages.join(','));
      }
    );
  }

  getCategories() {
    this.api.GetCategories(
      data => (this.categories = data),
      error => console.log(error)
    );
  }

  getConditions() {
    this.api.GetConditions({},
      data => (this.conditions = data),
      error => console.log(error)
    );
  }

  UserHasConditionsToConvertTo() {
    this.api.GetConditions({
      UserId: this.User.UserId
    },
      data => {
        this.userFromConditions = data.filter(x => x.Convertible == true && x.UserConvertibleFrom == true).map(x => x.ConditionId);
        var any = data.filter(x => x.Convertible == true && x.UserConvertibleTo == true);
        this.userHasConditionsToConvertTo = (any.length != 0);
      },
      error => {
        console.log(error);
      }
    );
  }

  UserHasLocationsToConvertFrom() {

    this.api.GetSublocations({
      UserId: this.User.UserId,
      OnlySublocation: true
    },1,//channel id here
     data => {

      var any = data.filter(x => x.Status == 1 && x.ConvertibleFrom == true && x.UserConvertibleFrom == true && x.ParentLocation != null);
      this.userHasLocationsToConvertFrom = (any.length != 0);

    }, error => {
      console.log(error);
    });



    this.api.GetConditions({
      UserId: this.User.UserId
    },
      data => {
        this.userFromConditions = data.filter(x => x.Convertible == true && x.UserConvertibleFrom == true).map(x => x.ConditionId);
        var any = data.filter(x => x.Convertible == true && x.UserConvertibleTo == true);
        this.userHasConditionsToConvertTo = (any.length != 0);
      },
      error => {
        console.log(error);
      }
    );
  }

  getProductGroups() {
    this.api.GetProductGroups(
      data => (this.productGroups = data),
      error => console.log(error)
    );
  }

  getPrinters() {
    this.api.GetPrinters({OnlyAssingedPrinters: true},
      data => {
        this.printers = data.List;
      },
      error => console.log(error)
    );
  }

  Sync(){
    this.syncing = true;
    this.api.Sync(1, //channel id goes here
      
      data => {
      this.getProducts();
      this.getCategories();
      this.getConditions();
      this.GetLastSynced();
      this.syncing = false;

    }, error => {
      alert("there was an error while trying to sync");
      this.syncing = false;
      console.log(error);
    })
  }

  GetLastSynced() {
    this.api.GetLastSync("SyncProducts",
      data => {
        this.lastSynced = data;
      },
      error => console.log(error)
    );
  }

  SortBy(sort){

    if(sort == this.Criteria.SortBy) {
      this.Criteria.Ascending = !this.Criteria.Ascending;

    } else {
      this.Criteria.Ascending = true;
    }

    this.Criteria.SortBy = sort;
    this.getProducts();

   /*var CriteriaCopy ={...this.Criteria}
    CriteriaCopy.SortBy = sort;
    console.log(CriteriaCopy)
    var test = sort + "Bool";

    Criteria.Ascending =!Criteria.Ascending
    this.Criteria.SortBy = sort;*/
    //  
  }
test(){
  this.focus = true;
  console.log("fsfsf", this.focus)
}



}
