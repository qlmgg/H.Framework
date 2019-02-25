﻿using H.Framework.Core.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace H.Framework.Data.ORM.Foundations
{
    public abstract class FoundationBLL<TViewModel, TModel, TDAL> : IFoundationBLL<TViewModel>
                                                                where TModel : IFoundationModel, new()
                                                                where TDAL : FoundationDAL<TModel>
                                                                where TViewModel : IFoundationViewModel, new()
    {
        public TDAL DAL { get; set; }

        public FoundationBLL()
        {
            DAL = Activator.CreateInstance<TDAL>();
        }

        public virtual void Add(List<TViewModel> list)
        {
            Add(list, Selector);
        }

        public virtual void Add(TViewModel model)
        {
            Add(new List<TViewModel> { model }, Selector);
        }

        protected void Add(List<TViewModel> list, Func<TViewModel, TModel> selector)
        {
            if (list == null) return;
            DAL.Add(list.MapAllTo(selector));
        }

        public virtual void Delete(TViewModel model)
        {
            DAL.DeleteLogic(new TModel { ID = model.ID });
        }

        public virtual void Update(List<TViewModel> list, string include = "")
        {
            Update(list, Selector, include);
        }

        public virtual void Update(TViewModel model, string include = "")
        {
            Update(new List<TViewModel> { model }, Selector, include);
        }

        protected void Update(List<TViewModel> list, Func<TViewModel, TModel> selector, string include = "")
        {
            if (list == null) return;
            DAL.Update(list.MapAllTo(selector), include);
        }

        public virtual TViewModel Get(Expression<Func<TViewModel, bool>> whereSelector, string include = "", OrderByEntity orderBy = null)
        {
            var item = DAL.Get(MySQLUtility.GetModelExpr<TViewModel, TModel>(whereSelector), include, orderBy);
            if (item != null)
                return item.MapTo(RetrieveSelector);
            else
                return default(TViewModel);
        }

        public virtual Task<TViewModel> GetAsync(Expression<Func<TViewModel, bool>> whereSelector, string include = "", OrderByEntity orderBy = null)
        {
            return Task.Run(() => Get(whereSelector, include, orderBy));
        }

        public virtual List<TViewModel> GetList(Expression<Func<TViewModel, bool>> whereSelector, string include = "", OrderByEntity orderBy = null)
        {
            return DAL.GetList(MySQLUtility.GetModelExpr<TViewModel, TModel>(whereSelector), include, orderBy).MapAllTo(RetrieveSelector).ToList();
        }

        public virtual Task<List<TViewModel>> GetListAsync(Expression<Func<TViewModel, bool>> whereSelector, string include = "", OrderByEntity orderBy = null)
        {
            return Task.Run(() => DAL.GetList(MySQLUtility.GetModelExpr<TViewModel, TModel>(whereSelector), include, orderBy).MapAllTo(RetrieveSelector).ToList());
        }

        public virtual List<TViewModel> GetList(Expression<Func<TViewModel, bool>> whereSelector, int pageSize, int pageNum, string include = "", OrderByEntity orderBy = null)
        {
            return DAL.GetList(MySQLUtility.GetModelExpr<TViewModel, TModel>(whereSelector), pageSize, pageNum, include, orderBy).MapAllTo(RetrieveSelector).ToList();
        }

        public virtual Task<List<TViewModel>> GetListAsync(Expression<Func<TViewModel, bool>> whereSelector, int pageSize, int pageNum, string include = "", OrderByEntity orderBy = null)
        {
            return Task.Run(() => GetList(whereSelector, pageSize, pageNum, include, orderBy));
        }

        public virtual int Count(Expression<Func<TViewModel, bool>> whereSelector)
        {
            return DAL.Count(MySQLUtility.GetModelExpr<TViewModel, TModel>(whereSelector));
        }

        public virtual Task<int> CountAsync(Expression<Func<TViewModel, bool>> whereSelector)
        {
            return Task.Run(() => Count(whereSelector));
        }

        public virtual TViewModel Get(WhereQueryable<TViewModel> whereSelector, string include = "", OrderByEntity orderBy = null)
        {
            return Get(whereSelector.Expr, include, orderBy);
        }

        public virtual Task<TViewModel> GetSync(WhereQueryable<TViewModel> whereSelector, string include = "", OrderByEntity orderBy = null)
        {
            return Task.Run(() => Get(whereSelector.Expr, include, orderBy));
        }

        public virtual List<TViewModel> GetList(WhereQueryable<TViewModel> whereSelector, string include = "", OrderByEntity orderBy = null)
        {
            return GetList(whereSelector.Expr, include, orderBy);
        }

        public virtual Task<List<TViewModel>> GetListAsync(WhereQueryable<TViewModel> whereSelector, string include = "", OrderByEntity orderBy = null)
        {
            return GetListAsync(whereSelector.Expr, include, orderBy);
        }

        public virtual List<TViewModel> GetList(WhereQueryable<TViewModel> whereSelector, int pageSize, int pageNum, string include = "", OrderByEntity orderBy = null)
        {
            return GetList(whereSelector.Expr, pageSize, pageNum, include, orderBy);
        }

        public virtual Task<List<TViewModel>> GetListAsync(WhereQueryable<TViewModel> whereSelector, int pageSize, int pageNum, string include = "", OrderByEntity orderBy = null)
        {
            return GetListAsync(whereSelector.Expr, pageSize, pageNum, include, orderBy);
        }

        public virtual int Count(WhereQueryable<TViewModel> whereSelector)
        {
            return Count(whereSelector.Expr);
        }

        public virtual Task<int> CountAsync(WhereQueryable<TViewModel> whereSelector)
        {
            return Task.Run(() => Count(whereSelector.Expr));
        }

        //public List<TViewModel> ExecuteQuerySQL(string sqlText)
        //{
        //    return ExecuteQuerySQL(sqlText, RetrieveSelector);
        //}

        //protected List<TViewModel> ExecuteQuerySQL(string sqlText, Func<TModel, TViewModel> selector)
        //{
        //    return DAL.ExecuteQuerySQL(sqlText).MapAllTo(RetrieveSelector).ToList();
        //}

        protected Func<TModel, TViewModel> RetrieveSelector
        {
            get;
            set;
        } = item => new TViewModel();

        protected Func<TViewModel, TModel> Selector
        {
            get;
            set;
        } = item => new TModel();
    }

    public class ConvertMemberVisitor<TViewModel> : ExpressionVisitor
    {
        private readonly ParameterExpression[] _parms;

        public ConvertMemberVisitor(params ParameterExpression[] parms)
        {
            _parms = parms;
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var member = _parms[0].Type.GetMember(node.Member.Name).FirstOrDefault();
            if (!(node.Expression is ParameterExpression) || member == null)
            {
                object value = null, model = null;
                if (node.Expression is MemberExpression)
                {
                    var expr = VisitMember(node.Expression as MemberExpression);
                    if (expr is ConstantExpression)
                        model = (expr as ConstantExpression).Value;
                }
                if (node.Expression is ConstantExpression)
                    model = (node.Expression as ConstantExpression).Value;
                if (node.Member is FieldInfo)
                    value = ((FieldInfo)node.Member).GetValue(model);
                if (node.Member is PropertyInfo)
                    value = ((PropertyInfo)node.Member).GetValue(model);
                return Expression.Constant(value, value.GetType());
            }
            return Expression.MakeMemberAccess(_parms[0], member);
        }

        protected Expression ChildVisitMember(MemberExpression node)
        {
            object value = null, model = null;
            if (node.Expression is MemberExpression)
            {
                var expr = VisitMember(node.Expression as MemberExpression);
                if (expr is ConstantExpression)
                    model = (expr as ConstantExpression).Value;
            }
            if (node.Expression is ConstantExpression)
                model = (node.Expression as ConstantExpression).Value;
            if (node.Member is FieldInfo)
                value = ((FieldInfo)node.Member).GetValue(model);
            if (node.Member is PropertyInfo)
                value = ((PropertyInfo)node.Member).GetValue(model);
            return Expression.Constant(value, value.GetType());
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object is MemberExpression ndoe_expr)
            {
                return base.VisitMethodCall(node);
            }
            else
            {
                //if (node.Method.Name == "Contains")
                //{
                //    var arg = node.Arguments.FirstOrDefault(args => args.NodeType == ExpressionType.NewArrayInit);
                //    if (arg != null)
                //    {
                //        var arrExpr = arg as NewArrayExpression;
                //        var builder = new StringBuilder("");
                //        foreach (var arrItemExpr in arrExpr.Expressions)
                //        {
                //            if (arrItemExpr is MemberExpression)
                //            {
                //                var expr = VisitMember(arrItemExpr as MemberExpression);
                //                if (expr is ConstantExpression)
                //                    builder.Append("'" + (expr as ConstantExpression).Value + "',");
                //            }
                //            if (arrItemExpr is ConstantExpression)
                //            {
                //                builder.Append("'" + (arrItemExpr as ConstantExpression).Value + "',");
                //            }
                //        }
                //        return Expression.Constant(builder.Remove(builder.Length - 1, 1).ToString());
                //    }
                //}
                ;
                var value = Expression.Lambda(node, _parms).Compile().DynamicInvoke();
                return Expression.Constant(value, value.GetType());
            }
        }
    }
}