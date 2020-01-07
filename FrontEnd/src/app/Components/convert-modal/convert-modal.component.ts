import { Component, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ApiService } from 'src/app/services/api.service';
import { Condition } from 'selenium-webdriver';
import { SelectorMatcher } from '@angular/compiler';
import { Observable } from 'rxjs';
import { mergeMap } from 'rxjs/operators';
import { TypeaheadMatch, TypeaheadDirective } from 'ngx-bootstrap';

@Component({
  selector: 'app-convert-modal',
  templateUrl: './convert-modal.component.html',
  styleUrls: ['./convert-modal.component.css']
})
export class ConvertModalComponent implements OnInit {

  @Output() action = new EventEmitter();

  conversion: any = {
    Note: "",
    Quantity: null,
    PoNumber: null,
    FromSublocation: {},
    ToSublocation: {},
    ConvertType: "condition"
  };
  convertPlus = false;
  product: any;
  productSelected: string;
  productsDataSource: Observable<any>;
  User: any= {};
  conditions: Array<any> = [];
  sublocations: Array<any> = [];
  destSublocations: Array<any> = [];
  printWhenDone: boolean;
  transfer: boolean = false;
  status: string = "ready";
  toFromMatch = false;
  seelctedConditionId: string;
  selectedCondition = {};
  errorMessage: string;
  isNew = false;
  errorItems: any[];
  loading: boolean = false;
  condLoading: boolean = false;

  constructor(
    public modalRef: BsModalRef,
    private api: ApiService
  ) {
  }

  ngOnInit() {
    this.api.GetCurrentUsers(
      data => {
        this.User = data;
        this.getSourceSublocations();
        this.getConditions();
        this.convertPlus = this.User.Roles.includes('ConvertPlus');
      },
      error => console.log(error)
    );

    this.conversion.FromProduct = this.product;
    this.conversion.ToProduct = {
      ProductId: this.product.ProductId,
      Condition: {}
    };
    
    this.productsDataSource = Observable.create((observer: any) => {
      observer.next(this.productSelected);
    })
    .pipe(mergeMap((token: string) => this.api.FilterProducts(token)));
  }

  // getConditions() {
  //   console.log(this.conditions);
  //   return this.conditions.filter(x => x.Convertible == true && x.UserConvertibleTo == true);
  // }

  getConditions() {
    this.condLoading = true;
    this.api.GetConditions({
      UserId: this.User.UserId
    },
      data => {
        this.conditions = data.filter(x => x.Convertible == true && x.UserConvertibleTo == true);
        this.condLoading = false;
      },
      error => {
        console.log(error);
        this.condLoading = false;
      }
    );
  }
  
  getSourceSublocations() {
    let $this = this;
    let Criteria = {
      ProductId: this.product.ProductId,
      UserId: this.User.UserId,
      OnlySublocation: true
    }

    this.loading = true;
    $this.api.GetSublocations(
      Criteria, 
      1,//channel id here
      data => {
        $this.sublocations = data.filter(x => x.ConvertibleFrom == true && x.UserConvertibleFrom == true && x.Status == 1 && x.ParentLocation != null).sort(function(a, b) {
          return b.Stock - a.Stock;
        });

        $this.destSublocations = data.filter(x => x.ConvertibleTo == true && x.UserConvertibleTo == true && x.Status == 1 && x.ParentLocation != null).sort(function(a, b) {
          return b.Stock - a.Stock;
        });
        this.loading = false;
      },
      error => {
        console.log(error);
        this.loading = false;
      }
    );
  }

  getDestinationSublocations() {
    let $this = this;
    let Criteria = {
      ProductId: this.conversion.ToProduct.ProductId,
      UserId: this.User.UserId,
      OnlySublocation: true
    }

    $this.api.GetSublocations(
      Criteria,
      1,//channel id here
      data => {
        $this.destSublocations = data.filter(x => x.ConvertibleTo == true && x.UserConvertibleTo == true && x.Status == 1 && x.ParentLocation != null).sort(function(a, b) {
          return b.Stock - a.Stock;
        });
      },
      error => {

      }
    );
  }

  // getFromSublocations() {
  //   return this.sublocations.filter(x => x.Stock != 0).sort(function(a, b) {
  //     return b.Stock - a.Stock;
  //   });
  // }

  conditionChanged() {

    if(this.seelctedConditionId != '0') {

      this.errorMessage = null;
      this.selectedCondition = {};
      this.conversion.CreateNewProduct = false;
      this.conversion.ToProduct = {
        ProductId: this.product.ProductId,
        Condition: {}
      };
   
      if(this.product.Ncs == '') {
        this.errorMessage = "Error: This product does not have an NCS. Please add an NCS before using it to convert.";
      } else if(this.product.Condition == null) {
        this.errorMessage = "Error: This product does not have a condition. Please add a condition before using it to convert.";
      } else {
        let $this = this;
        let search = {
          Ncs: this.product.Ncs,
          Conditions: this.seelctedConditionId
        }
  
        this.api.SearchProducts(
          search, 1, //here goes the channel id
          data => {
  
            if(data.List.length < 1) {
              
              this.conversion.CreateNewProduct = true;
              this.selectedCondition = this.conditions.find(c => c.ConditionId == this.seelctedConditionId);
  
            } else {
  
              if(data.List.length > 1) {
  
                $this.errorMessage = "Error: There is more than one product that matches this NCS and condition. Please fix the following products:";
                $this.errorItems = data.List.map(p => " " + p.FinaleId);
  
              } else {
  
                this.conversion.ToProduct = data.List[0];
                
                let to = this.getProdName(this.conversion.ToProduct.Name);
                let from = this.getProdName(this.product.Name);
                this.toFromMatch = to[0].trim() === from[0].trim();
                this.selectedCondition = this.conditions.find(c => c.ConditionId == this.seelctedConditionId);
  
                $this.getDestinationSublocations();
              }
            }
            
          },
          error => {
            alert("Error: " + error.Messages.join(','));
          }
        );
      }
    }
  }

  convertTypeChanged() {

    this.errorMessage = null;
    this.selectedCondition = {};
    this.seelctedConditionId = '';
    this.productSelected = '';
    this.conversion.CreateNewProduct = false;
    
    if(this.conversion.ConvertType == 'condition') {

      this.conversion.ToProduct = {
        ProductId: this.product.ProductId,
        Condition: {}
      };

    } else {
      this.conversion.ToProduct = {
        Condition: {}
      };
    }
  }

  selectProduct(e: TypeaheadMatch): void {
    
    this.conversion.ToProduct = e.item;

    let to = this.getProdName(this.conversion.ToProduct.Name);
    let from = this.getProdName(this.product.Name);
    this.toFromMatch = to[0].trim() === from[0].trim();
    this.conversion.CreateNewProduct = false;

    this.getDestinationSublocations();
  }
  productChanged() {

    if(this.productSelected == '') {

      this.errorMessage = null;
      this.selectedCondition = {};
      this.conversion.CreateNewProduct = false;
      this.conversion.ToProduct = {
        Condition: {}
      };
    }
  }

  getProdName(product) {
    let regex = /.+?(?=\([^)]*\))/;
    return product.match(regex) || [product];
  }

  transferChanged() {
    if(!this.transfer) {
      this.conversion.ToSublocation = {...this.conversion.FromSublocation};
    }
  }

  convert() {
    var $this = this;
    $this.status = "converting";

    if($this.conversion.ConvertType == 'condition') {
      $this.conversion.ToProduct.Condition = $this.selectedCondition;
    }

    if($this.conversion.ToSublocation.SublocationId == null) {
      $this.conversion.ToSublocation = {...$this.conversion.FromSublocation};
    }

    $this.api.ConvertStock(
      $this.conversion, 1, //ChannelId goes here
      data => {
        $this.status = "converted";
        $this.conversion = data;
      },
      error => {
        alert("Error: " + error.Messages.join(','));
        $this.status = "ready";
      }
    );
  }

  print() {
    this.printWhenDone = true;
  }

  done(print) {
    this.modalRef.hide();
    this.action.emit({printWhenDone: print, conversion: this.conversion});
  }

}
