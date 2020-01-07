import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';

export interface PreferencesTab {
  path: string;
  label: string;
}

export type NavigationType = 'printers' | 'products';

const PrinterTabs = [{
  path: '/preferences/printer-setup',
  label: 'Manage Printers'
},
{
  path: '/preferences/label-setup',
  label: 'Printer Setup'
},
{
  path: '/preferences/print-log',
  label: 'Print Log'
}];

const ProductTabs = [{
  path: '/preferences/locations',
  label: 'Manage Locations'
}, {
  path: '/preferences/conditions',
  label: 'Manage Conditions'
}, {
  path: '/preferences/avg-cost-log',
  label: 'Average Cost Log'
}];

@Component({
  selector: 'app-preferences-navigation',
  templateUrl: './preferences-navigation.component.html',
})
export class PreferencesNavigationComponent {
  @Input() navigationType: NavigationType;
  private preferencesTabs: { [s in NavigationType]: PreferencesTab[] } = {
    printers: PrinterTabs,
    products: ProductTabs,
  };

  constructor(private router: Router) {
  }

  get tabs() {
    return this.preferencesTabs[this.navigationType] || [];
  }

  isTabActive(tabLink: string) {
    return this.router.isActive(tabLink, true);
  }

}
