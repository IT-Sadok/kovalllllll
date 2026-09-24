import React from 'react';
import { COMPONENT_TYPES, OTHER_CATEGORIES } from '../types';
import { CATEGORY_LABELS } from '../utils/componentSpecs';

const CategoryOptions: React.FC = () => (
  <>
    <optgroup label="Drone components">
      {COMPONENT_TYPES.map((c) => <option key={c} value={c}>{CATEGORY_LABELS[c]}</option>)}
    </optgroup>
    <optgroup label="Other">
      {OTHER_CATEGORIES.map((c) => <option key={c} value={c}>{CATEGORY_LABELS[c]}</option>)}
    </optgroup>
  </>
);

export default CategoryOptions;
